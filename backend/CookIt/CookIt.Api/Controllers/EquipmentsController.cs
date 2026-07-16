using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EquipmentsController : ControllerBase
    {
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<EquipmentsController> _logger;

        public EquipmentsController(
            IEquipmentService equipmentService,
            ILogger<EquipmentsController> logger)
        {
            _equipmentService = equipmentService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogDebug("Запрос всего оборудования");

            try
            {
                var equipments = await _equipmentService.GetAllEquipmentsAsync();

                _logger.LogInformation(
                    "Успешно получено {Count} единиц оборудования",
                    equipments.Count());

                return Ok(equipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении оборудования");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}