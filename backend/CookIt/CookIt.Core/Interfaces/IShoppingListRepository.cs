using CookIt.Core.Entities;

public interface IShoppingListRepository
{
    Task<ShoppingList?> GetByIdAsync(int id);
    Task<List<ShoppingList>> GetByUserIdAsync(string userId);
    Task<ShoppingList> AddAsync(ShoppingList shoppingList);
    Task UpdateAsync(ShoppingList shoppingList);
    Task DeleteAsync(ShoppingList shoppingList);
    Task<ShoppingListExcludedIngredient?> GetExcludedIngredientAsync(int shoppingListId, int ingredientId);
    Task AddExcludedIngredientAsync(ShoppingListExcludedIngredient excluded);
    Task RemoveExcludedIngredientAsync(ShoppingListExcludedIngredient excluded);
}