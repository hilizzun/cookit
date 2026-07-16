using CookIt.Core.Dtos.Ratings;

namespace CookIt.Core.Interfaces
{
    public interface IRatingService
    {
        Task RateRecipeAsync(string userId, int recipeId, int value);
        Task<RecipeRatingSummaryDto> GetRatingSummaryAsync(int recipeId, string? userId = null);
        Task<int?> GetUserRatingAsync(string userId, int recipeId);
        Task<bool> RemoveRatingAsync(string userId, int recipeId);
    }
}