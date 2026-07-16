using CookIt.Core.Dtos.Admin;
using CookIt.Core.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin,Moderator")]
    public class DishTypesController : ControllerBase
    {
        private readonly IAdminDishTypeService _adminDishTypeService;
        private readonly ILogger<DishTypesController> _logger;

        public DishTypesController(
            IAdminDishTypeService adminDishTypeService,
            ILogger<DishTypesController> logger)
        {
            _adminDishTypeService = adminDishTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<DishTypeAdminDto>>> GetAll(
            [FromQuery] bool includeDeleted = false)
        {
            _logger.LogDebug(
                "Административный запрос всех типов блюд. Включены удаленные: {IncludeDeleted}",
                includeDeleted);

            try
            {
                var dishTypes = await _adminDishTypeService.GetAllDishTypesAsync(includeDeleted);

                _logger.LogInformation(
                    "Успешно получено {Count} типов блюд. Включены удаленные: {IncludeDeleted}",
                    dishTypes.Count, includeDeleted);

                return Ok(dishTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка типов блюд");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DishTypeAdminDto>> GetById(int id)
        {
            _logger.LogDebug("Запрос типа блюда по ID: {Id}", id);

            try
            {
                var dishType = await _adminDishTypeService.GetDishTypeByIdAsync(id);

                _logger.LogInformation("Тип блюда с ID {Id} успешно получен", id);
                return Ok(dishType);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Тип блюда с ID {Id} не найден. Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении типа блюда с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<DishTypeAdminDto>> Create([FromBody] CreateDishTypeDto dto)
        {
            _logger.LogInformation(
                "Создание нового типа блюда. Название: {Name}",
                dto.Name);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при создании типа блюда. Название: {Name}, Ошибки: {@Errors}",
                        dto.Name, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var dishType = await _adminDishTypeService.CreateDishTypeAsync(dto);

                _logger.LogInformation(
                    "Тип блюда успешно создан. ID: {Id}, Название: {Name}",
                    dishType.Id, dishType.Name);

                return CreatedAtAction(nameof(GetById), new { id = dishType.Id }, dishType);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при создании типа блюда. Название: {Name}, Ошибка: {ErrorMessage}",
                    dto.Name, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании типа блюда. Название: {Name}", dto.Name);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DishTypeAdminDto>> Update(int id, [FromBody] UpdateDishTypeDto dto)
        {
            _logger.LogInformation(
                "Обновление типа блюда. ID: {Id}, Новое название: {Name}",
                id, dto.Name);

            try
            {
                if (id != dto.Id)
                {
                    _logger.LogWarning(
                        "Идентификаторы не совпадают при обновлении типа блюда. ID в пути: {PathId}, ID в теле: {BodyId}",
                        id, dto.Id);
                    return BadRequest(new { message = "Идентификаторы не совпадают" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при обновлении типа блюда. ID: {Id}, Ошибки: {@Errors}",
                        id, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var dishType = await _adminDishTypeService.UpdateDishTypeAsync(dto);

                _logger.LogInformation(
                    "Тип блюда успешно обновлен. ID: {Id}, Новое название: {Name}",
                    dishType.Id, dishType.Name);

                return Ok(dishType);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при обновлении типа блюда. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении типа блюда с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning("Логическое удаление типа блюда. ID: {Id}", id);

            try
            {
                await _adminDishTypeService.DeleteDishTypeAsync(id);

                _logger.LogWarning("Тип блюда логически удалён. ID: {Id}", id);
                return Ok(new { message = "Тип блюда логически удалён" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при удалении типа блюда. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении типа блюда с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Восстановление типа блюда. ID: {Id}", id);

            try
            {
                await _adminDishTypeService.RestoreDishTypeAsync(id);

                _logger.LogInformation("Тип блюда восстановлен. ID: {Id}", id);
                return Ok(new { message = "Тип блюда восстановлен" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при восстановлении типа блюда. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при восстановлении типа блюда с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}/is-used")]
        public async Task<ActionResult<bool>> IsUsed(int id)
        {
            _logger.LogDebug("Проверка использования типа блюда. ID: {Id}", id);

            try
            {
                var isUsed = await _adminDishTypeService.IsDishTypeUsedAsync(id);

                _logger.LogDebug(
                    "Тип блюда {Id} используется: {IsUsed}",
                    id, isUsed);

                return Ok(isUsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке использования типа блюда с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}