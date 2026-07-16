using CookIt.Core.Entities;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _repository;

        public EquipmentService(IEquipmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Equipment>> GetAllEquipmentsAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
