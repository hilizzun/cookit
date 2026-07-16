using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShoppingListController : ControllerBase
{
    private readonly IShoppingListService _shoppingListService;

    public ShoppingListController(IShoppingListService shoppingListService)
    {
        _shoppingListService = shoppingListService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetShoppingList()
    {
        var list = await _shoppingListService.GetUserShoppingListAsync(GetUserId());
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> AddRecipe([FromQuery] int recipeId, [FromQuery] double servings = 1)
    {
        if (servings <= 0) return BadRequest("Количество порций должно быть положительным");
        await _shoppingListService.AddRecipeToListAsync(GetUserId(), recipeId, servings);
        return Ok();
    }

    [HttpPut("{shoppingListId}/servings")]
    public async Task<IActionResult> UpdateServings(int shoppingListId, [FromQuery] double servings)
    {
        if (servings <= 0) return BadRequest("Количество порций должно быть положительным");
        await _shoppingListService.UpdateServingsAsync(shoppingListId, servings);
        return NoContent();
    }

    [HttpDelete("{shoppingListId}")]
    public async Task<IActionResult> RemoveRecipe(int shoppingListId)
    {
        await _shoppingListService.RemoveRecipeFromListAsync(shoppingListId);
        return NoContent();
    }

    [HttpPatch("{shoppingListId}/exclude-ingredient")]
    public async Task<IActionResult> ToggleExcludeIngredient(int shoppingListId, [FromQuery] int ingredientId)
    {
        await _shoppingListService.ToggleExcludeIngredientAsync(shoppingListId, ingredientId);
        return NoContent();
    }
}