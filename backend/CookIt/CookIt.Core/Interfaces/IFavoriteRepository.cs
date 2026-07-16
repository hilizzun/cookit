public interface IFavoriteRepository
{
    Task AddFavoriteAsync(string userId, int recipeId);
    Task RemoveFavoriteAsync(string userId, int recipeId);
    Task<bool> IsFavoriteAsync(string userId, int recipeId);
    Task<IEnumerable<int>> GetFavoriteRecipeIdsAsync(string userId);
}