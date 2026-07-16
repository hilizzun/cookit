using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DishTypesController : ControllerBase
    {
        private readonly IDishTypeService _dishTypeService;
        private readonly ILogger<DishTypesController> _logger;

        public DishTypesController(
            IDishTypeService dishTypeService,
            ILogger<DishTypesController> logger)
        {
            _dishTypeService = dishTypeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogDebug("Запрос всех типов блюд");

            try
            {
                var dishTypes = await _dishTypeService.GetAllDishTypesAsync();

                _logger.LogInformation(
                    "Успешно получено {Count} типов блюд",
                    dishTypes.Count());

                return Ok(dishTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении типов блюд");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}