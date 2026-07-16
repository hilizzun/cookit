using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IIngredientService
    {
        Task<IEnumerable<Ingredient>> GetAllIngredientsAsync();
        Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string searchText);
    }
}
