using CookIt.Core.Dtos.Admin;
using CookIt.Core.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin,Moderator")]
    public class EquipmentsController : ControllerBase
    {
        private readonly IAdminEquipmentService _adminEquipmentService;
        private readonly ILogger<EquipmentsController> _logger;

        public EquipmentsController(
            IAdminEquipmentService adminEquipmentService,
            ILogger<EquipmentsController> logger)
        {
            _adminEquipmentService = adminEquipmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<EquipmentAdminDto>>> GetAll(
            [FromQuery] bool includeDeleted = false)
        {
            _logger.LogDebug(
                "Административный запрос всего оборудования. Включены удаленные: {IncludeDeleted}",
                includeDeleted);

            try
            {
                var equipments = await _adminEquipmentService.GetAllEquipmentsAsync(includeDeleted);

                _logger.LogInformation(
                    "Успешно получено {Count} единиц оборудования. Включены удаленные: {IncludeDeleted}",
                    equipments.Count, includeDeleted);

                return Ok(equipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка оборудования");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentAdminDto>> GetById(int id)
        {
            _logger.LogDebug("Запрос оборудования по ID: {Id}", id);

            try
            {
                var equipment = await _adminEquipmentService.GetEquipmentByIdAsync(id);

                _logger.LogInformation("Оборудование с ID {Id} успешно получено", id);
                return Ok(equipment);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Оборудование с ID {Id} не найдено. Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении оборудования с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<EquipmentAdminDto>> Create([FromBody] CreateEquipmentDto dto)
        {
            _logger.LogInformation(
                "Создание нового оборудования. Название: {Name}",
                dto.Name);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при создании оборудования. Название: {Name}, Ошибки: {@Errors}",
                        dto.Name, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var equipment = await _adminEquipmentService.CreateEquipmentAsync(dto);

                _logger.LogInformation(
                    "Оборудование успешно создано. ID: {Id}, Название: {Name}",
                    equipment.Id, equipment.Name);

                return CreatedAtAction(nameof(GetById), new { id = equipment.Id }, equipment);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при создании оборудования. Название: {Name}, Ошибка: {ErrorMessage}",
                    dto.Name, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании оборудования. Название: {Name}", dto.Name);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EquipmentAdminDto>> Update(int id, [FromBody] UpdateEquipmentDto dto)
        {
            _logger.LogInformation(
                "Обновление оборудования. ID: {Id}, Новое название: {Name}",
                id, dto.Name);

            try
            {
                if (id != dto.Id)
                {
                    _logger.LogWarning(
                        "Идентификаторы не совпадают при обновлении оборудования. ID в пути: {PathId}, ID в теле: {BodyId}",
                        id, dto.Id);
                    return BadRequest(new { message = "Идентификаторы не совпадают" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning(
                        "Невалидные данные при обновлении оборудования. ID: {Id}, Ошибки: {@Errors}",
                        id, ModelState.Values.SelectMany(v => v.Errors));
                    return BadRequest(ModelState);
                }

                var equipment = await _adminEquipmentService.UpdateEquipmentAsync(dto);

                _logger.LogInformation(
                    "Оборудование успешно обновлено. ID: {Id}, Новое название: {Name}",
                    equipment.Id, equipment.Name);

                return Ok(equipment);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при обновлении оборудования. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении оборудования с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning("Помечание оборудования как удаленного. ID: {Id}", id);

            try
            {
                await _adminEquipmentService.DeleteEquipmentAsync(id);

                _logger.LogWarning("Оборудование помечено как удаленное. ID: {Id}", id);
                return Ok(new { message = "Оборудование помечено как удаленное" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при удалении оборудования. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении оборудования с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            _logger.LogInformation("Восстановление оборудования. ID: {Id}", id);

            try
            {
                await _adminEquipmentService.RestoreEquipmentAsync(id);

                _logger.LogInformation("Оборудование восстановлено. ID: {Id}", id);
                return Ok(new { message = "Оборудование восстановлено" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка при восстановлении оборудования. ID: {Id}, Ошибка: {ErrorMessage}",
                    id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при восстановлении оборудования с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet("{id}/is-used")]
        public async Task<ActionResult<bool>> IsUsed(int id)
        {
            _logger.LogDebug("Проверка использования оборудования. ID: {Id}", id);

            try
            {
                var isUsed = await _adminEquipmentService.IsEquipmentUsedAsync(id);

                _logger.LogDebug(
                    "Оборудование {Id} используется: {IsUsed}",
                    id, isUsed);

                return Ok(isUsed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке использования оборудования с id: {Id}", id);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}