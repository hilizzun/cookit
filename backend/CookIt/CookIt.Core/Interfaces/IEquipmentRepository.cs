using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IEquipmentRepository
    {
        Task<IEnumerable<Equipment>> GetAllAdminAsync(bool includeDeleted);
        Task<IEnumerable<Equipment>> GetAllAsync();
        Task<Equipment?> GetByIdAsync(int id);
        Task<Equipment?> GetByIdAdminAsync(int id, bool includeDeleted = false);
        Task<bool> IsEquipmentUsedAsync(int id);
        Task<Equipment> AddAsync(Equipment equipment);
        Task<Equipment> UpdateAsync(Equipment equipment);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int id);
    }
}