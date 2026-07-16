using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CookIt.Application.BackgroundServices
{
    public class FactGenerationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FactGenerationService> _logger;
        private readonly double _intervalHours;

        public FactGenerationService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<FactGenerationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _intervalHours = configuration.GetValue<double>("FactGeneration:IntervalHours", 1.0);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис генерации фактов запущен. Интервал: {Interval} ч.", _intervalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateFactsForMissingEntities();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при выполнении генерации фактов");
                }

                await Task.Delay(TimeSpan.FromHours(_intervalHours), stoppingToken);
            }

            _logger.LogInformation("Сервис генерации фактов остановлен.");
        }

        private async Task GenerateFactsForMissingEntities()
        {
            using var scope = _serviceProvider.CreateScope();
            var recipeRepo = scope.ServiceProvider.GetRequiredService<IRecipeRepository>();
            var ingredientRepo = scope.ServiceProvider.GetRequiredService<IIngredientRepository>();
            var factRepo = scope.ServiceProvider.GetRequiredService<IInterestingFactRepository>();
            var aiService = scope.ServiceProvider.GetRequiredService<IAiService>();

            var recipeIdsWithoutFacts = await factRepo.GetRecipeIdsWithoutFactsAsync();
            var recipesWithoutFacts = new List<RecipeDto>();

            foreach (var id in recipeIdsWithoutFacts)
            {
                var recipeDto = await recipeRepo.GetByIdAsync(id);
                if (recipeDto != null)
                    recipesWithoutFacts.Add(recipeDto);
            }

            foreach (var recipeDto in recipesWithoutFacts)
            {
                _logger.LogInformation("Генерация фактов для рецепта: {RecipeName}", recipeDto.Name);
                var facts = await aiService.GetFactsForRecipeAsync(recipeDto.Name);
                foreach (var fact in facts)
                {
                    await factRepo.AddFactAsync(new InterestingFact
                    {
                        EntityType = "Recipe",
                        EntityId = recipeDto.Id,
                        FactText = fact,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await factRepo.SaveChangesAsync();
                await Task.Delay(1000);
            }

            var ingredientIdsWithoutFacts = await factRepo.GetIngredientIdsWithoutFactsAsync();
            var ingredientsWithoutFacts = new List<Ingredient>();

            foreach (var id in ingredientIdsWithoutFacts)
            {
                var ingredient = await ingredientRepo.GetByIdAsync(id);
                if (ingredient != null)
                    ingredientsWithoutFacts.Add(ingredient);
            }

            foreach (var ingredient in ingredientsWithoutFacts)
            {
                _logger.LogInformation("Генерация фактов для ингредиента: {IngredientName}", ingredient.Name);
                var facts = await aiService.GetFactsForIngredientAsync(ingredient.Name);
                foreach (var fact in facts)
                {
                    await factRepo.AddFactAsync(new InterestingFact
                    {
                        EntityType = "Ingredient",
                        EntityId = ingredient.Id,
                        FactText = fact,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await factRepo.SaveChangesAsync();
                await Task.Delay(1000);
            }

            _logger.LogInformation("Генерация фактов завершена.");
        }
    }
}