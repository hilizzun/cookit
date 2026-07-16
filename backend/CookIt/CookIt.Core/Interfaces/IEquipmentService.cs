using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IEquipmentService
    {
        Task<IEnumerable<Equipment>> GetAllEquipmentsAsync(); 
    }
}
