using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IUnitService
    {
        Task<IEnumerable<Unit>> GetAllUnitsAsync();
    }
}