using CookIt.Core.Dtos.Admin;
using CookIt.Core.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin,Moderator")]
    public class UnitsController : ControllerBase
    {
        private readonly IAdminUnitService _adminUnitService;
        private readonly ILogger<UnitsController> _logger;

        public UnitsController(
            IAdminUnitService adminUnitService,
            ILogger<UnitsController> logger)
        {
            _adminUnitService = adminUnitService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<UnitAdminDto>>> GetAll(
            [FromQuery] bool includeDeleted = false)
        {
            _logger.LogDebug(
                "Административный запрос всех единиц измерения. Включены удаленные: {IncludeDeleted}",
                includeDeleted);

            try
            {
                var units = await _adminUnitService.GetAllUnitsAsync(includeDeleted);

                _logger.LogInformation(
                    "Успешно получено {Count} единиц измерения. Включены удаленные: {IncludeDeleted}",
                    units.Count, includeDeleted);

                return Ok(units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка единиц измерения");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UnitAdminDto>> GetById(int id)
        {
            _logger.LogDebug("Запрос единицы измерения по ID: {Id}", id);

            try
            {
                var unit = await _adminUnitService.GetUnitByIdAsync(id);

                _logger.LogInformation("Единица измерения с ID {Id} успешно получена", id);
                return Ok(unit);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Единица измерения с ID {Id} не найдена. Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении единицы измерения с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UnitAdminDto>> Create([FromBody] CreateUnitDto dto)
        {
            _logger.LogInformation(
                "Создание новой единицы измерения. Название: {Name}, Конверсия в граммы: {ConversionToGrams}",
                dto.Name, dto.ConversionToGrams);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при создании единицы измерения. Название: {Name}, Ошибки: {@Errors}",
                        dto.Name, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var unit = await _adminUnitService.CreateUnitAsync(dto);

                _logger.LogInformation(
                    "Единица измерения успешно создана. ID: {Id}, Название: {Name}",
                    unit.Id, unit.Name);

                return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при создании единицы измерения. Название: {Name}, Ошибка: {ErrorMessage}",
                    dto.Name, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании единицы измерения. Название: {Name}", dto.Name);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UnitAdminDto>> Update(int id, [FromBody] UpdateUnitDto dto)
        {
            _logger.LogInformation(
                "Обновление единицы измерения. ID: {Id}, Новое название: {Name}",
                id, dto.Name);

            try
            {
                if (id != dto.Id)
                {
                    _logger.LogWarning(
                        "Идентификаторы не совпадают при обновлении единицы измерения. ID в пути: {PathId}, ID в теле: {BodyId}",
                        id, dto.Id);
                    return BadRequest(new { message = "Идентификаторы не совпадают" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при обновлении единицы измерения. ID: {Id}, Ошибки: {@Errors}",
                        id, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var unit = await _adminUnitService.UpdateUnitAsync(dto);

                _logger.LogInformation(
                    "Единица измерения успешно обновлена. ID: {Id}, Новое название: {Name}",
                    unit.Id, unit.Name);

                return Ok(unit);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при обновлении единицы измерения. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении единицы измерения с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning("Помечание единицы измерения как удаленной. ID: {Id}", id);

            try
            {
                await _adminUnitService.DeleteUnitAsync(id);

                _logger.LogWarning("Единица измерения помечена как удаленная. ID: {Id}", id);
                return Ok(new { message = "Единица измерения помечена как удаленная" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при удалении единицы измерения. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении единицы измерения с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Восстановление единицы измерения. ID: {Id}", id);

            try
            {
                await _adminUnitService.RestoreUnitAsync(id);

                _logger.LogInformation("Единица измерения восстановлена. ID: {Id}", id);
                return Ok(new { message = "Единица измерения восстановлена" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при восстановлении единицы измерения. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при восстановлении единицы измерения с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}/is-used")]
        public async Task<ActionResult<bool>> IsUsed(int id)
        {
            _logger.LogDebug("Проверка использования единицы измерения. ID: {Id}", id);

            try
            {
                var isUsed = await _adminUnitService.IsUnitUsedAsync(id);

                _logger.LogDebug(
                    "Единица измерения {Id} используется: {IsUsed}",
                    id, isUsed);

                return Ok(isUsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке использования единицы измерения с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}