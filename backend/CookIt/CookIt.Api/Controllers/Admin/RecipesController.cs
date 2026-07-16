using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookIt.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin,Moderator")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(
            IRecipeService recipeService,
            ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _logger = logger;
        }

        [HttpGet("moderation")]
        public async Task<ActionResult<List<RecipeModerationListDto>>> GetRecipesForModeration()
        {
            var moderatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Модератор {ModeratorId} запрашивает рецепты для модерации",
                moderatorId);

            try
            {
                var recipes = await _recipeService.GetRecipesForModerationAsync();

                _logger.LogInformation(
                    "Модератор {ModeratorId} получил {Count} рецептов для модерации",
                    moderatorId, recipes.Count());

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении рецептов для модерации модератором {ModeratorId}",
                    moderatorId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // Принятие/отклонение рецепта
        [HttpPost("moderate")]
        public async Task<IActionResult> ModerateRecipe([FromBody] ModerateRecipeDto dto)
        {
            var moderatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Модерация рецепта {RecipeId} модератором {ModeratorId}. Решение: {IsApproved}, Комментарий: {Comment}",
                dto.RecipeId, moderatorId, dto.IsApproved, dto.RejectionComment);

            try
            {
                if (string.IsNullOrEmpty(moderatorId))
                {
                    _logger.LogWarning("Попытка модерации без аутентификации");
                    return Unauthorized();
                }

                var result = await _recipeService.ModerateRecipeAsync(dto, moderatorId);
                if (!result)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден или уже прошел модерацию",
                        dto.RecipeId);
                    return NotFound(new { message = "Рецепт не найден или уже прошел модерацию" });
                }

                var action = dto.IsApproved ? "принят" : "отклонен";

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно {Action} модератором {ModeratorId}",
                    dto.RecipeId, action, moderatorId);

                return Ok(new { message = $"Рецепт успешно {action}" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка модерации рецепта {RecipeId}. Ошибка: {ErrorMessage}",
                    dto.RecipeId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при модерации рецепта {RecipeId} модератором {ModeratorId}",
                    dto.RecipeId, moderatorId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        // Повторная отправка рецепта на модерацию
        [HttpPost("{id}/resubmit")]
        public async Task<IActionResult> ResubmitForModeration(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Повторная отправка рецепта {RecipeId} на модерацию пользователем {UserId}",
                id, userId);

            try
            {
                // Проверяем, что пользователь является автором рецепта
                if (!await _recipeService.IsCreatorAsync(id, userId))
                {
                    _logger.LogWarning(
                        "Пользователь {UserId} не автор рецепта {RecipeId} при повторной отправке на модерацию",
                        userId, id);
                    return Forbid();
                }

                var result = await _recipeService.ResubmitForModerationAsync(id);
                if (!result)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден при повторной отправке на модерацию",
                        id);
                    return NotFound(new { message = "Рецепт не найден" });
                }

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно отправлен на повторную модерацию пользователем {UserId}",
                    id, userId);

                return Ok(new { message = "Рецепт отправлен на повторную модерацию" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при повторной отправке рецепта {RecipeId} на модерацию пользователем {UserId}",
                    id, userId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("moderation/{id}")]
        public async Task<ActionResult<RecipeDto>> GetRecipeForModeration(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Запрос рецепта {RecipeId} для модерации модератором {UserId}",
                id, userId);

            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id, userId);

                if (recipe == null)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден для модерации",
                        id);
                    return NotFound(new { message = "Рецепт не найден" });
                }

                if (recipe.IsApproved != null)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} уже прошел модерацию. Статус: {IsApproved}",
                        id, recipe.IsApproved);
                    return BadRequest(new { message = "Рецепт уже прошел модерацию" });
                }

                _logger.LogInformation(
                    "Рецепт {RecipeId} получен для модерации модератором {UserId}",
                    id, userId);

                return Ok(recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении рецепта {RecipeId} для модерации модератором {UserId}",
                    id, userId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}