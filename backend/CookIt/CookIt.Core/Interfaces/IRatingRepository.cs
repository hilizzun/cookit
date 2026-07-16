using CookIt.Core.Dtos.Ratings;
using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IRatingRepository
    {
        Task<RecipeRating?> GetUserRatingAsync(string userId, int recipeId);
        Task AddOrUpdateRatingAsync(string userId, int recipeId, int value);
        Task<RecipeRatingSummaryDto> GetRatingSummaryAsync(int recipeId, string? userId = null);
        Task<double> GetAverageRatingAsync(int recipeId);
        Task<int> GetTotalRatingsCountAsync(int recipeId);
        Task<bool> DeleteRatingAsync(string userId, int recipeId);
        Task<bool> HasUserRatedRecipeAsync(string userId, int recipeId);
    }
}