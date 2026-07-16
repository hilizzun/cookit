using CookIt.Core.Dtos.ShopingList;

namespace CookIt.Core.Interfaces
{
    public interface IShoppingListService
    {
        Task<List<ShoppingListRecipeDto>> GetUserShoppingListAsync(string userId);
        Task AddRecipeToListAsync(string userId, int recipeId, double servings);
        Task UpdateServingsAsync(int shoppingListId, double servings);
        Task RemoveRecipeFromListAsync(int shoppingListId);
        Task ToggleExcludeIngredientAsync(int shoppingListId, int ingredientId);
    }
}
