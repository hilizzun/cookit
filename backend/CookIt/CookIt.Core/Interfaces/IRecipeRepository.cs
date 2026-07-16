using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{

    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllApprovedAsync();
        Task<(IEnumerable<RecipeDto> Recipes, int TotalCount)> GetAllAsync(RecipeFilterDto? filter = null, string? userId = null, bool includeUnapproved = false);
        Task<RecipeDto?> GetByIdAsync(int id);
        Task<RecipeDto> AddAsync(CreateRecipeDto recipeDto, string creatorId, string? imageKey = null, bool isModerator = false);
        Task<RecipeDto> UpdateAsync(int id, UpdateRecipeDto recipeDto, bool resetModeration = false);
        Task<bool> UpdateImagePathAsync(int id, string imagePath);
        Task<IEnumerable<RecipeDto>> GetRecentRecipesAsync();
        Task<IEnumerable<RecipeDto>> GetTopRecipesAsync();
        Task DeleteAsync(int id);
        Task<bool> IsCreatorAsync(int recipeId, string userId);
        Task<IEnumerable<RecipeDto>> GetByIdsAsync(IEnumerable<int> recipeIds);
        Task<IEnumerable<RecipeDto>> GetRecipesByUserIdAsync(string userId, bool includeAllStatuses = false);
        Task<double> GetUserRecipesAverageRatingAsync(string userId);
        Task<int> GetUserRecipesCountAsync(string userId);
        Task<int> GetUserRecipesTotalRatingsAsync(string userId);
        Task UpdateNutritionAsync(int recipeId, double totalCalories, double totalProteins, double totalFats, double totalCarbs,
        double caloriesPerServing, double proteinsPerServing, double fatsPerServing, double carbsPerServing,
        double caloriesPer100g, double proteinsPer100g, double fatsPer100g, double carbsPer100g);
        Task<IEnumerable<RecipeModerationListDto>> GetRecipesForModerationAsync();
        Task<Recipe?> GetRecipeForModerationAsync(int id);
        Task<bool> ModerateRecipeAsync(int recipeId, bool isApproved, string moderatorId, string? rejectionComment = null);
        Task<bool> ResubmitForModerationAsync(int recipeId);
        Task<List<int>> GetIngredientIdsByRecipeIdAsync(int recipeId);
        Task<IEnumerable<Recipe>> GetRandomApprovedRecipesAsync(int count);
    }
}
