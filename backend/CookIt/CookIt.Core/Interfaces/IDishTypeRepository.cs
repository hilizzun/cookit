using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IDishTypeRepository
    {
        Task<IEnumerable<DishType>> GetAllAdminAsync(bool includeDeleted);
        Task<IEnumerable<DishType>> GetAllAsync();
        Task<DishType?> GetByIdAsync(int id);
        Task<DishType?> GetByIdAdminAsync(int id, bool includeDeleted = false);
        Task<bool> IsDishTypeUsedAsync(int id);
        Task<DishType> AddAsync(DishType dishType);
        Task<DishType> UpdateAsync(DishType dishType);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}