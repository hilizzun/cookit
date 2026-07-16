using CookIt.Application.Services;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WheelController : ControllerBase
{
    private readonly IUserStatisticsService _userStatisticsService;
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<WheelController> _logger;

    public WheelController(
        IUserStatisticsService userStatisticsService,
        IRecipeRepository recipeRepository,
        ILogger<WheelController> logger)
    {
        _userStatisticsService = userStatisticsService;
        _recipeRepository = recipeRepository;
        _logger = logger;
    }

    [HttpPost("spin")]
    public async Task<IActionResult> Spin()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _userStatisticsService.IncrementWheelSpinsAsync(userId);

        var randomRecipes = await _recipeRepository.GetRandomApprovedRecipesAsync(1);
        var recipe = randomRecipes.FirstOrDefault();
        if (recipe == null)
            return NotFound("Нет доступных рецептов");

        _logger.LogInformation("Пользователь {UserId} прокрутил рулетку, выпал рецепт {RecipeId}", userId, recipe.Id);
        return Ok(recipe);
    }
}