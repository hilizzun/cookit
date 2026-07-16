using CookIt.Core.Entities;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services
{
    public class DishTypeService : IDishTypeService
    {
        private readonly IDishTypeRepository _repository;

        public DishTypeService(IDishTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DishType>> GetAllDishTypesAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
