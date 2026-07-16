using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FactsController : ControllerBase
{
    private readonly IFactService _factService;

    public FactsController(IFactService factService)
    {
        _factService = factService;
    }

    [HttpGet("recipe/{recipeId}")]
    public async Task<ActionResult<IEnumerable<string>>> GetFactsForRecipe(int recipeId)
    {
        var facts = await _factService.GetFactsForRecipeAsync(recipeId);
        return Ok(facts);
    }
}