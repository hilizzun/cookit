using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IDishTypeService
    {
        Task<IEnumerable<DishType>> GetAllDishTypesAsync();
    }
}
