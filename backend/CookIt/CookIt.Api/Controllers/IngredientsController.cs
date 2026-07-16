using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;
        private readonly ILogger<IngredientsController> _logger;

        public IngredientsController(
            IIngredientService ingredientService,
            ILogger<IngredientsController> logger)
        {
            _ingredientService = ingredientService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogDebug("Запрос всех ингредиентов");

            try
            {
                var ingredientsList = await _ingredientService.GetAllIngredientsAsync();

                _logger.LogInformation(
                    "Успешно получено {Count} ингредиентов",
                    ingredientsList.Count());

                return Ok(ingredientsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ингредиентов");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search([FromQuery] string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                _logger.LogWarning("Пустой поисковый запрос ингредиентов");
                return BadRequest("Search text is required");
            }

            _logger.LogDebug("Поиск ингредиентов. Текст: {SearchText}", searchText);

            try
            {
                var ingredients = await _ingredientService.SearchIngredientsAsync(searchText);

                _logger.LogInformation(
                    "Поиск ингредиентов завершен. Текст: {SearchText}, Найдено: {Count}",
                    searchText, ingredients.Count());

                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при поиске ингредиентов. Текст: {SearchText}", searchText);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}