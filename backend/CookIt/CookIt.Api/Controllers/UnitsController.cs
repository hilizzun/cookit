using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(
        IUnitService unitService,
        ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogDebug("Запрос всех единиц измерения");

        try
        {
            var units = await _unitService.GetAllUnitsAsync();

            _logger.LogInformation(
                "Успешно получено {Count} единиц измерения",
                units.Count());

            return Ok(units);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении единиц измерения");
            return StatusCode(500, "Internal server error");
        }
    }
}