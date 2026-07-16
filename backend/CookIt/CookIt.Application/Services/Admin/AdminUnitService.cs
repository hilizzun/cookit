using CookIt.Core.Dtos.Admin;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces.Admin;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services.Admin
{
    public class AdminUnitService : IAdminUnitService
    {
        private readonly IUnitRepository _repository;

        public AdminUnitService(IUnitRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UnitAdminDto>> GetAllUnitsAsync(bool includeDeleted = false)
        {
            var units = await _repository.GetAllAdminAsync(includeDeleted);

            var result = new List<UnitAdminDto>();
            foreach (var unit in units)
            {
                var isUsed = await _repository.IsUnitUsedAsync(unit.Id);
                result.Add(new UnitAdminDto
                {
                    Id = unit.Id,
                    Name = unit.Name,
                    ConversionToGrams = unit.ConversionToGrams,
                    IsDeleted = unit.IsDeleted,
                    IsUsedInRecipes = isUsed
                });
            }

            return result;
        }

        public async Task<UnitAdminDto> GetUnitByIdAsync(int id)
        {
            var unit = await _repository.GetByIdAdminAsync(id, true);
            if (unit == null)
                throw new ApplicationException("Единица измерения не найдена");

            var isUsed = await _repository.IsUnitUsedAsync(id);

            return new UnitAdminDto
            {
                Id = unit.Id,
                Name = unit.Name,
                ConversionToGrams = unit.ConversionToGrams,
                IsDeleted = unit.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<UnitAdminDto> CreateUnitAsync(CreateUnitDto dto)
        {
            var unit = new Unit
            {
                Name = dto.Name,
                ConversionToGrams = dto.ConversionToGrams,
                IsDeleted = false
            };

            var createdUnit = await _repository.AddAsync(unit);

            return new UnitAdminDto
            {
                Id = createdUnit.Id,
                Name = createdUnit.Name,
                ConversionToGrams = createdUnit.ConversionToGrams,
                IsDeleted = createdUnit.IsDeleted,
                IsUsedInRecipes = false
            };
        }

        public async Task<UnitAdminDto> UpdateUnitAsync(UpdateUnitDto dto)
        {
            var unit = await _repository.GetByIdAdminAsync(dto.Id, true);
            if (unit == null)
                throw new ApplicationException("Единица измерения не найдена");

            if (unit.IsDeleted)
                throw new ApplicationException("Нельзя редактировать удаленную единицу измерения");

            // Проверяем, используется ли единица измерения в рецептах
            var isUsed = await _repository.IsUnitUsedAsync(dto.Id);
            if (isUsed)
                throw new ApplicationException("Нельзя редактировать единицу измерения, которая используется в рецептах");

            unit.Name = dto.Name;
            unit.ConversionToGrams = dto.ConversionToGrams;

            var updatedUnit = await _repository.UpdateAsync(unit);

            return new UnitAdminDto
            {
                Id = updatedUnit.Id,
                Name = updatedUnit.Name,
                ConversionToGrams = updatedUnit.ConversionToGrams,
                IsDeleted = updatedUnit.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<bool> DeleteUnitAsync(int id)
        {
            return await _repository.SoftDeleteAsync(id);
        }

        public async Task<bool> RestoreUnitAsync(int id)
        {
            return await _repository.RestoreAsync(id);
        }

        public async Task<bool> IsUnitUsedAsync(int id)
        {
            return await _repository.IsUnitUsedAsync(id);
        }
    }
}