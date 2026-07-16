using CookIt.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(IFavoriteService favoriteService, ILogger<FavoritesController> logger)
    {
        _favoriteService = favoriteService;
        _logger = logger;
    }

    [HttpPost("{recipeId}")]
    public async Task<IActionResult> AddToFavorites(int recipeId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            await _favoriteService.AddToFavoritesAsync(userId, recipeId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении в избранное");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{recipeId}")]
    public async Task<IActionResult> RemoveFromFavorites(int recipeId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            await _favoriteService.RemoveFromFavoritesAsync(userId, recipeId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении из избранного");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{recipeId}")]
    public async Task<IActionResult> IsFavorite(int recipeId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isFavorite = await _favoriteService.IsFavoriteAsync(userId, recipeId);
        return Ok(isFavorite);
    }

    [HttpGet]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var recipes = await _favoriteService.GetUserFavoritesAsync(userId);
        return Ok(recipes);
    }
}