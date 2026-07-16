using CookIt.Core.Dtos.Admin;
using CookIt.Core.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin,Moderator")]
    public class IngredientsController : ControllerBase
    {
        private readonly IAdminIngredientService _adminIngredientService;
        private readonly ILogger<IngredientsController> _logger;
        private readonly IAiService _aiService;

        public IngredientsController(
            IAdminIngredientService adminIngredientService,
            ILogger<IngredientsController> logger,
            IAiService aiService)
        {
            _adminIngredientService = adminIngredientService;
            _logger = logger;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<ActionResult<List<IngredientAdminDto>>> GetAll(
            [FromQuery] bool includeDeleted = false)
        {
            _logger.LogDebug(
                "Административный запрос всех ингредиентов. Включены удаленные: {IncludeDeleted}",
                includeDeleted);

            try
            {
                var ingredients = await _adminIngredientService.GetAllIngredientsAsync(includeDeleted);

                _logger.LogInformation(
                    "Успешно получено {Count} ингредиентов. Включены удаленные: {IncludeDeleted}",
                    ingredients.Count, includeDeleted);

                return Ok(ingredients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка ингредиентов");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IngredientAdminDto>> GetById(int id)
        {
            _logger.LogDebug("Запрос ингредиента по ID: {Id}", id);

            try
            {
                var ingredient = await _adminIngredientService.GetIngredientByIdAsync(id);

                _logger.LogInformation("Ингредиент с ID {Id} успешно получен", id);
                return Ok(ingredient);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Ингредиент с ID {Id} не найден. Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении ингредиента с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<IngredientAdminDto>> Create([FromBody] CreateIngredientDto dto)
        {
            _logger.LogInformation(
                "Создание нового ингредиента. Название: {Name}, Ккал: {Calories}, Белки: {Proteins}, Жиры: {Fats}, Углеводы: {Carbohydrates}",
                dto.Name, dto.Calories, dto.Proteins, dto.Fats, dto.Carbohydrates);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при создании ингредиента. Название: {Name}, Ошибки: {@Errors}",
                        dto.Name, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var ingredient = await _adminIngredientService.CreateIngredientAsync(dto);

                _logger.LogInformation(
                    "Ингредиент успешно создан. ID: {Id}, Название: {Name}",
                    ingredient.Id, ingredient.Name);

                return CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, ingredient);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при создании ингредиента. Название: {Name}, Ошибка: {ErrorMessage}",
                    dto.Name, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании ингредиента. Название: {Name}", dto.Name);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IngredientAdminDto>> Update(int id, [FromBody] UpdateIngredientDto dto)
        {
            _logger.LogInformation(
                "Обновление ингредиента. ID: {Id}, Новое название: {Name}",
                id, dto.Name);

            try
            {
                if (id != dto.Id)
                {
                    _logger.LogWarning(
                        "Идентификаторы не совпадают при обновлении ингредиента. ID в пути: {PathId}, ID в теле: {BodyId}",
                        id, dto.Id);
                    return BadRequest(new { message = "Идентификаторы не совпадают" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при обновлении ингредиента. ID: {Id}, Ошибки: {@Errors}",
                        id, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var ingredient = await _adminIngredientService.UpdateIngredientAsync(dto);

                _logger.LogInformation(
                    "Ингредиент успешно обновлен. ID: {Id}, Новое название: {Name}",
                    ingredient.Id, ingredient.Name);

                return Ok(ingredient);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при обновлении ингредиента. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении ингредиента с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning("Помечание ингредиента как удаленного. ID: {Id}", id);

            try
            {
                await _adminIngredientService.DeleteIngredientAsync(id);

                _logger.LogWarning("Ингредиент помечен как удаленный. ID: {Id}", id);
                return Ok(new { message = "Ингредиент помечен как удаленный" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при удалении ингредиента. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении ингредиента с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Восстановление ингредиента. ID: {Id}", id);

            try
            {
                await _adminIngredientService.RestoreIngredientAsync(id);

                _logger.LogInformation("Ингредиент восстановлен. ID: {Id}", id);
                return Ok(new { message = "Ингредиент восстановлен" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при восстановлении ингредиента. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при восстановлении ингредиента с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}/is-used")]
        public async Task<ActionResult<bool>> IsUsed(int id)
        {
            _logger.LogDebug("Проверка использования ингредиента. ID: {Id}", id);

            try
            {
                var isUsed = await _adminIngredientService.IsIngredientUsedAsync(id);

                _logger.LogDebug(
                    "Ингредиент {Id} используется: {IsUsed}",
                    id, isUsed);

                return Ok(isUsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке использования ингредиента с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("auto-nutrition")]
        public async Task<IActionResult> GetAutoNutrition([FromBody] AutoNutritionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Название ингредиента обязательно");

            try
            {
                var nutrition = await _aiService.GetIngredientNutritionAsync(request.Name);
                return Ok(nutrition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении КБЖУ для {Name}", request.Name);
                return StatusCode(500, "Не удалось получить данные");
            }
        }

        public class AutoNutritionRequest
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}