using CookIt.Core.Dtos.Ratings;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookIt.Api.Controllers
{
    [ApiController]
    [Route("api/recipes/{recipeId}/ratings")]
    [Authorize]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(
            IRatingService ratingService,
            IRecipeService recipeService,
            ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _recipeService = recipeService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> RateRecipe(int recipeId, [FromBody] RecipeRatingDto ratingDto)
        {
            if (ratingDto.Value < 1 || ratingDto.Value > 5)
            {
                _logger.LogWarning(
                    "Некорректная оценка рецепта {RecipeId}. Оценка: {Rating}",
                    recipeId, ratingDto.Value);
                return BadRequest("Rating value must be between 1 and 5");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Оценка рецепта {RecipeId} пользователем {UserId}. Оценка: {Rating}",
                recipeId, userId, ratingDto.Value);

            try
            {
                await _ratingService.RateRecipeAsync(userId, recipeId, ratingDto.Value);

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно оценен пользователем {UserId}. Оценка: {Rating}",
                    recipeId, userId, ratingDto.Value);

                return Ok(new { message = "Recipe rated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(
                    "Ошибка оценки рецепта {RecipeId}. Пользователь: {UserId}, Ошибка: {ErrorMessage}",
                    recipeId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(
                    "Невозможно оценить рецепт {RecipeId}. Пользователь: {UserId}, Ошибка: {ErrorMessage}",
                    recipeId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при оценке рецепта {RecipeId} пользователем {UserId}",
                    recipeId, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateRating(int recipeId, [FromBody] RecipeRatingDto ratingDto)
        {
            _logger.LogDebug("Обновление оценки рецепта {RecipeId}", recipeId);
            return await RateRecipe(recipeId, ratingDto);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingSummary(int recipeId)
        {
            _logger.LogDebug("Запрос сводки оценок рецепта {RecipeId}", recipeId);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var ratingSummary = await _ratingService.GetRatingSummaryAsync(recipeId, userId);

                _logger.LogDebug(
                    "Сводка оценок рецепта {RecipeId}: Средняя оценка: {AverageRating}, Всего оценок: {TotalRatings}",
                    recipeId, ratingSummary.AverageRating, ratingSummary.TotalRatings);

                return Ok(ratingSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сводки оценок рецепта {RecipeId}", recipeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRating(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Запрос оценки пользователя {UserId} для рецепта {RecipeId}",
                userId, recipeId);

            try
            {
                var userRating = await _ratingService.GetUserRatingAsync(userId, recipeId);

                _logger.LogDebug(
                    "Оценка пользователя {UserId} для рецепта {RecipeId}: {Rating}",
                    userId, recipeId, userRating);

                return Ok(new { Rating = userRating });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении оценки пользователя {UserId} для рецепта {RecipeId}",
                    userId, recipeId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveRating(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Удаление оценки рецепта {RecipeId} пользователем {UserId}",
                recipeId, userId);

            try
            {
                var result = await _ratingService.RemoveRatingAsync(userId, recipeId);
                if (!result)
                {
                    _logger.LogWarning(
                        "Оценка не найдена при удалении. Рецепт: {RecipeId}, Пользователь: {UserId}",
                        recipeId, userId);
                    return NotFound("Rating not found");
                }

                _logger.LogInformation(
                    "Оценка рецепта {RecipeId} успешно удалена пользователем {UserId}",
                    recipeId, userId);

                return Ok(new { message = "Rating removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при удалении оценки рецепта {RecipeId} пользователем {UserId}",
                    recipeId, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("can-rate")]
        public async Task<IActionResult> CanRateRecipe(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Проверка возможности оценки рецепта {RecipeId} пользователем {UserId}",
                recipeId, userId);

            try
            {
                bool isCreator = await _recipeService.IsCreatorAsync(recipeId, userId);
                if (isCreator)
                {
                    _logger.LogDebug(
                        "Пользователь {UserId} не может оценить свой собственный рецепт {RecipeId}",
                        userId, recipeId);
                    return Ok(new { CanRate = false, Reason = "You cannot rate your own recipe" });
                }

                var recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
                if (recipe == null)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден при проверке возможности оценки",
                        recipeId);
                    return Ok(new { CanRate = false, Reason = "Recipe not found" });
                }

                _logger.LogDebug(
                    "Пользователь {UserId} может оценить рецепт {RecipeId}",
                    userId, recipeId);

                return Ok(new { CanRate = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при проверке возможности оценки рецепта {RecipeId} пользователем {UserId}",
                    recipeId, userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}