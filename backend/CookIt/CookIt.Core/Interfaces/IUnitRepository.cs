using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IUnitRepository
    {
        Task<IEnumerable<Unit>> GetAllAdminAsync(bool includeDeleted);
        Task<IEnumerable<Unit>> GetAllAsync();
        Task<Unit?> GetByIdAsync(int id);
        Task<Unit?> GetByIdAdminAsync(int id, bool includeDeleted = false);
        Task<bool> IsUnitUsedAsync(int id);
        Task<Unit> AddAsync(Unit unit);
        Task<Unit> UpdateAsync(Unit unit);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}