using CookIt.Core.Dtos.Admin;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces.Admin;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services.Admin
{
    public class AdminEquipmentService : IAdminEquipmentService
    {
        private readonly IEquipmentRepository _repository;

        public AdminEquipmentService(IEquipmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EquipmentAdminDto>> GetAllEquipmentsAsync(bool includeDeleted = false)
        {
            var equipments = await _repository.GetAllAdminAsync(includeDeleted);

            var result = new List<EquipmentAdminDto>();
            foreach (var equipment in equipments)
            {
                var isUsed = await _repository.IsEquipmentUsedAsync(equipment.Id);
                result.Add(new EquipmentAdminDto
                {
                    Id = equipment.Id,
                    Name = equipment.Name,
                    IsDeleted = equipment.IsDeleted,
                    IsUsedInRecipes = isUsed
                });
            }

            return result;
        }

        public async Task<EquipmentAdminDto> GetEquipmentByIdAsync(int id)
        {
            var equipment = await _repository.GetByIdAdminAsync(id, true);
            if (equipment == null)
                throw new ApplicationException("Оборудование не найдено");

            var isUsed = await _repository.IsEquipmentUsedAsync(id);

            return new EquipmentAdminDto
            {
                Id = equipment.Id,
                Name = equipment.Name,
                IsDeleted = equipment.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<EquipmentAdminDto> CreateEquipmentAsync(CreateEquipmentDto dto)
        {
            var equipment = new Equipment
            {
                Name = dto.Name,
                IsDeleted = false
            };

            var createdEquipment = await _repository.AddAsync(equipment);

            return new EquipmentAdminDto
            {
                Id = createdEquipment.Id,
                Name = createdEquipment.Name,
                IsDeleted = createdEquipment.IsDeleted,
                IsUsedInRecipes = false
            };
        }

        public async Task<EquipmentAdminDto> UpdateEquipmentAsync(UpdateEquipmentDto dto)
        {
            var equipment = await _repository.GetByIdAdminAsync(dto.Id, true);
            if (equipment == null)
                throw new ApplicationException("Оборудование не найдено");

            if (equipment.IsDeleted)
                throw new ApplicationException("Нельзя редактировать удаленное оборудование");

            // Проверяем, используется ли оборудование в рецептах
            var isUsed = await _repository.IsEquipmentUsedAsync(dto.Id);
            if (isUsed)
                throw new ApplicationException("Нельзя редактировать оборудование, которое используется в рецептах");

            equipment.Name = dto.Name;

            var updatedEquipment = await _repository.UpdateAsync(equipment);

            return new EquipmentAdminDto
            {
                Id = updatedEquipment.Id,
                Name = updatedEquipment.Name,
                IsDeleted = updatedEquipment.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            return await _repository.SoftDeleteAsync(id);
        }

        public async Task<bool> RestoreEquipmentAsync(int id)
        {
            return await _repository.RestoreAsync(id);
        }

        public async Task<bool> IsEquipmentUsedAsync(int id)
        {
            return await _repository.IsEquipmentUsedAsync(id);
        }
    }
}