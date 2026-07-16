using CookIt.Core.Entities;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services
{
    public class UnitService : IUnitService
    {
        private readonly IUnitRepository _repository;

        public UnitService(IUnitRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<Unit>> GetAllUnitsAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
