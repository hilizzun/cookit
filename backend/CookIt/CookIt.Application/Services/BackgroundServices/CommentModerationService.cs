using CookIt.Core.Interfaces;
using CookIt.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class CommentModerationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommentModerationService> _logger;
    private readonly double _intervalHours;

    public CommentModerationService(IServiceProvider serviceProvider, IConfiguration config, ILogger<CommentModerationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _intervalHours = config.GetValue<double>("CommentModeration:IntervalHours", 1.0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис модерации комментариев запущен. Интервал: {Interval} ч.", _intervalHours);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ModerateNewCommentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при модерации комментариев");
            }
            await Task.Delay(TimeSpan.FromHours(_intervalHours), stoppingToken);
        }
    }

    private async Task ModerateNewCommentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
        var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepository>();
        var complaintRepo = scope.ServiceProvider.GetRequiredService<IComplaintRepository>();
        var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CookItContext>();

        var recipes = await recipeRepo.GetAllApprovedAsync();
        _logger.LogInformation("Модерация: получено {Count} рецептов", recipes.Count());

        foreach (var recipe in recipes)
        {
            var lastCheck = recipe.LastCommentsCheck;
            _logger.LogDebug("Рецепт {RecipeId}: LastCommentsCheck = {LastCheck}", recipe.Id, lastCheck);

            var newComments = await commentRepo.GetNewCommentsForRecipeAsync(recipe.Id, lastCheck);
            var commentsList = newComments.ToList();
            if (!commentsList.Any())
            {
                _logger.LogDebug("Рецепт {RecipeId}: новых комментариев нет", recipe.Id);
                continue;
            }

            _logger.LogInformation("Рецепт {RecipeId}: найдено {Count} новых комментариев", recipe.Id, commentsList.Count);

            foreach (var comment in commentsList)
            {
                _logger.LogInformation("Проверка комментария {CommentId}: {Content}", comment.Id, comment.Content);

                var result = await aiService.ModerateCommentAsync(comment.Content);
                _logger.LogInformation("Результат модерации: IsOffensive={IsOffensive}, IsOffTopic={IsOffTopic}, Reason={Reason}",
                    result.IsOffensive, result.IsOffTopic, result.Reason);

                if (result.IsOffensive || result.IsOffTopic)
                {
                    var reason = $"Автоматическая жалоба: {(result.IsOffensive ? "нецензурная лексика" : "")} {(result.IsOffTopic ? "оффтоп" : "")}".Trim();
                    _logger.LogInformation("Попытка создать жалобу для комментария {CommentId}, причина: {Reason}", comment.Id, reason);

                    try
                    {
                        await complaintRepo.CreateAutoComplaintAsync(comment.Id, reason);
                        _logger.LogWarning("Жалоба на комментарий {CommentId} успешно создана", comment.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ОШИБКА при создании жалобы для комментария {CommentId}", comment.Id);
                    }
                }
                else
                {
                    _logger.LogDebug("Комментарий {CommentId} чист", comment.Id);
                }
                await Task.Delay(500);
            }

            recipe.LastCommentsCheck = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Для рецепта {RecipeId} обновлена дата проверки", recipe.Id);
        }
    }
}