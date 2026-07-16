using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CookIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IMinioImageStorage _imageStorage;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(
            IRecipeService recipeService,
            IMinioImageStorage imageStorage,
            ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        [HttpGet("random-wheel")]
        [Authorize]
        public async Task<IActionResult> GetRandomRecipesForWheel([FromQuery] int count = 12)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Запрос случайных рецептов для колеса. Count: {Count}, UserId: {UserId}", count, userId);

            try
            {
                var recipes = await _recipeService.GetRandomRecipesForWheelAsync(count, userId);
                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении случайных рецептов");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] RecipeFilterDto filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Получение всех рецептов. Пользователь: {UserId}, Страница: {PageNumber}, Размер страницы: {PageSize}, Поиск: {SearchText}",
                userId, filter.PageNumber, filter.PageSize, filter.SearchText);

            try
            {
                var (recipes, totalCount) = await _recipeService.GetAllRecipesAsync(filter, userId);

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
                {
                    TotalCount = totalCount,
                    PageSize = filter.PageSize,
                    CurrentPage = filter.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                }));

                _logger.LogInformation(
                    "Успешно получено {RecipeCount} рецептов из {TotalCount}. Пользователь: {UserId}",
                    recipes.Count(), totalCount, userId);

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении рецептов. Пользователь: {UserId}, Фильтр: {@Filter}",
                    userId, filter);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Запрос рецепта {RecipeId} пользователем {UserId}",
                id, userId);

            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id, userId);
                if (recipe == null)
                {
                    var isCreator = await _recipeService.IsCreatorAsync(id, userId);
                    var isAdmin = User.IsInRole("Admin") || User.IsInRole("Moderator");

                    if (!isCreator && !isAdmin)
                    {
                        _logger.LogWarning(
                            "Попытка доступа к рецепту {RecipeId} пользователем {UserId} без прав. Пользователь не автор и не администратор",
                            id, userId);
                        return NotFound();
                    }

                    recipe = await _recipeService.GetRecipeByIdAsync(id, userId);
                    if (recipe == null)
                    {
                        _logger.LogWarning(
                            "Рецепт {RecipeId} не найден в базе данных",
                            id);
                        return NotFound();
                    }
                }

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно получен пользователем {UserId}",
                    id, userId);

                return Ok(recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении рецепта {RecipeId} пользователем {UserId}",
                    id, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] RecipeCreateRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Создание нового рецепта. Пользователь: {UserId}, Название: {RecipeName}, Тип блюда: {DishTypeId}, Ингредиентов: {IngredientCount}",
                userId, request.Name, request.DishTypeId, request.IngredientIds?.Count ?? 0);

            try
            {
                var recipe = await _recipeService.AddRecipeAsync(request, userId);

                _logger.LogInformation(
                    "Рецепт успешно создан. ID: {RecipeId}, Название: {RecipeName}, Пользователь: {UserId}",
                    recipe.Id, recipe.Name, userId);

                return CreatedAtAction(nameof(Get), new { id = recipe.Id }, recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при создании рецепта. Пользователь: {UserId}, Данные: {@Request}",
                    userId, new
                    {
                        request.Name,
                        request.DishTypeId,
                        request.CookingTimeWithUser,
                        request.CookingTimeWithoutUser,
                        request.SpicinessLevel,
                        request.DifficultyLevel
                    });
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateRecipeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Обновление рецепта {RecipeId}. Пользователь: {UserId}, Новое название: {RecipeName}",
                id, userId, request.Name);

            try
            {
                if (!await _recipeService.IsCreatorAsync(id, userId) && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
                {
                    _logger.LogWarning(
                        "Попытка обновления рецепта {RecipeId} пользователем {UserId} без прав",
                        id, userId);
                    return Forbid();
                }

                var updatedRecipe = await _recipeService.UpdateRecipeAsync(id, request);

                if (updatedRecipe == null)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден при попытке обновления пользователем {UserId}",
                        id, userId);
                    return NotFound();
                }

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно обновлен пользователем {UserId}",
                    id, userId);

                return Ok(updatedRecipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при обновлении рецепта {RecipeId}. Пользователь: {UserId}, Данные: {@Request}",
                    id, userId, new
                    {
                        request.Name,
                        request.DishTypeId
                    });
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Удаление рецепта {RecipeId}. Пользователь: {UserId}",
                id, userId);

            try
            {
                if (!await _recipeService.IsCreatorAsync(id, userId) && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning(
                        "Попытка удаления рецепта {RecipeId} пользователем {UserId} без прав",
                        id, userId);
                    return Forbid();
                }

                var existingRecipe = await _recipeService.GetRecipeByIdAsync(id);
                if (existingRecipe == null)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден при попытке удаления пользователем {UserId}",
                        id, userId);
                    return NotFound();
                }

                await _recipeService.DeleteRecipeAsync(id);

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно удален пользователем {UserId}",
                    id, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при удалении рецепта {RecipeId}. Пользователь: {UserId}",
                    id, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(int id, IFormFile image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Загрузка изображения для рецепта {RecipeId}. Пользователь: {UserId}, Размер файла: {FileSize}, Тип: {ContentType}",
                id, userId, image.Length, image.ContentType);

            try
            {
                if (!await _recipeService.IsCreatorAsync(id, userId) && !User.IsInRole("Admin"))
                {
                    _logger.LogWarning(
                        "Попытка загрузки изображения для рецепта {RecipeId} пользователем {UserId} без прав",
                        id, userId);
                    return Forbid();
                }

                var result = await _recipeService.UpdateRecipeImageAsync(id, image);

                if (!result)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден при загрузке изображения пользователем {UserId}",
                        id, userId);
                    return NotFound();
                }

                _logger.LogInformation(
                    "Изображение для рецепта {RecipeId} успешно загружено пользователем {UserId}",
                    id, userId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при загрузке изображения для рецепта {RecipeId}. Пользователь: {UserId}",
                    id, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("recent")]
        [Authorize]
        public async Task<IActionResult> GetRecentRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Получение последних рецептов. Пользователь: {UserId}",
                userId);

            try
            {
                var recipes = await _recipeService.GetRecentRecipesAsync(userId);

                _logger.LogInformation(
                    "Успешно получено {RecipeCount} последних рецептов. Пользователь: {UserId}",
                    recipes.Count(), userId);

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении последних рецептов. Пользователь: {UserId}",
                    userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("top")]
        [Authorize]
        public async Task<IActionResult> GetTopRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug(
                "Получение топовых рецептов. Пользователь: {UserId}",
                userId);

            try
            {
                var recipes = await _recipeService.GetTopRecipesAsync(userId);

                _logger.LogInformation(
                    "Успешно получено {RecipeCount} топовых рецептов. Пользователь: {UserId}",
                    recipes.Count(), userId);

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении топовых рецептов. Пользователь: {UserId}",
                    userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/image-url")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImageUrl(int id, [FromQuery] bool preview = true)
        {
            _logger.LogDebug(
                "Запрос URL изображения рецепта {RecipeId}, предпросмотр: {Preview}",
                id, preview);

            try
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(id);
                if (recipe == null || string.IsNullOrEmpty(recipe.ImagePath))
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} или его изображение не найдены при запросе URL",
                        id);
                    return NotFound();
                }

                string imageUrl = preview
                ? await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath)
                : await _imageStorage.GetOriginalUrlAsync(recipe.ImagePath);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning(
                        "Не удалось сгенерировать URL изображения для рецепта {RecipeId}",
                        id);
                    return NotFound();
                }

                _logger.LogDebug(
                    "Успешно сгенерирован URL изображения для рецепта {RecipeId}: {ImageUrl}",
                    id, imageUrl);

                return Ok(new { Url = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при генерации URL изображения для рецепта {RecipeId}",
                    id);
                return StatusCode(500, "Error generating image URL");
            }
        }

        [HttpPost("{id}/resubmit")]
        [Authorize]
        public async Task<IActionResult> ResubmitForModeration(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Повторная отправка рецепта {RecipeId} на модерацию. Пользователь: {UserId}",
                id, userId);

            try
            {
                if (!await _recipeService.IsCreatorAsync(id, userId))
                {
                    _logger.LogWarning(
                        "Попытка повторной отправки рецепта {RecipeId} на модерацию пользователем {UserId} без прав",
                        id, userId);
                    return Forbid();
                }

                var result = await _recipeService.ResubmitForModerationAsync(id);
                if (!result)
                {
                    _logger.LogWarning(
                        "Рецепт {RecipeId} не найден при повторной отправке на модерацию пользователем {UserId}",
                        id, userId);
                    return NotFound();
                }

                _logger.LogInformation(
                    "Рецепт {RecipeId} успешно отправлен на повторную модерацию пользователем {UserId}",
                    id, userId);

                return Ok(new { message = "Рецепт отправлен на повторную модерацию" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при повторной отправке рецепта {RecipeId} на модерацию. Пользователь: {UserId}",
                    id, userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}