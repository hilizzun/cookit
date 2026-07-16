using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Dtos.Users;
using Microsoft.AspNetCore.Http;
namespace CookIt.Core.Interfaces
{
    public interface IRecipeService
    {
        Task<IEnumerable<RecipeWheelItemDto>> GetRandomRecipesForWheelAsync(int count, string? userId = null);
        Task<(IEnumerable<RecipeDto> Recipes, int TotalCount)> GetAllRecipesAsync(
        RecipeFilterDto? filter = null,
        string? userId = null);
        Task<RecipeDto?> GetRecipeByIdAsync(int id, string? userId = null);
        Task<RecipeDto> AddRecipeAsync(RecipeCreateRequest request, string creatorId);
        Task<RecipeDto?> UpdateRecipeAsync(int id, UpdateRecipeRequest request);
        Task DeleteRecipeAsync(int id);
        Task<bool> UpdateRecipeImageAsync(int recipeId, IFormFile imageFile);
        Task<IEnumerable<RecipeDto>> GetRecentRecipesAsync(string? userId = null);
        Task<IEnumerable<RecipeDto>> GetTopRecipesAsync(string? userId = null);
        Task<bool> IsCreatorAsync(int id, string? userId);
        Task<IEnumerable<RecipeDto>> GetRecipesByIdsAsync(IEnumerable<int> recipeIds);
        Task<IEnumerable<RecipeDto>> GetRecipesByUserIdAsync(string userId);
        Task<UserRecipesSummaryDto> GetUserRecipesSummaryAsync(string userId);
        Task<IEnumerable<RecipeModerationListDto>> GetRecipesForModerationAsync();
        Task<bool> ModerateRecipeAsync(ModerateRecipeDto dto, string moderatorId);
        Task<bool> ResubmitForModerationAsync(int recipeId);
    }
}