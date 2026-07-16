using CookIt.Core.Dtos.Recipes;

public interface IFavoriteService
{
    Task AddToFavoritesAsync(string userId, int recipeId);
    Task RemoveFromFavoritesAsync(string userId, int recipeId);
    Task<bool> IsFavoriteAsync(string userId, int recipeId);
    Task<IEnumerable<RecipeDto>> GetUserFavoritesAsync(string userId);
}