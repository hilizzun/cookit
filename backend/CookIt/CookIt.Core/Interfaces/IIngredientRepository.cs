using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IIngredientRepository
    {
        Task<IEnumerable<Ingredient>> GetAllAdminAsync(bool includeDeleted);
        Task<IEnumerable<Ingredient>> GetAllAsync();
        Task<Ingredient?> GetByIdAsync(int id);
        Task<Ingredient?> GetByIdAdminAsync(int id, bool includeDeleted = false);
        Task<bool> IsIngredientUsedAsync(int id);
        Task<Ingredient> AddAsync(Ingredient ingredient);
        Task<Ingredient> UpdateAsync(Ingredient ingredient);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
        Task<IEnumerable<Ingredient>> SearchAsync(string searchText);
    }
}