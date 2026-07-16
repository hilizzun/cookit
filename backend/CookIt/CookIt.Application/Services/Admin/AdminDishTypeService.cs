using CookIt.Core.Dtos.Admin;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces.Admin;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services.Admin
{
    public class AdminDishTypeService : IAdminDishTypeService
    {
        private readonly IDishTypeRepository _repository;

        public AdminDishTypeService(IDishTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DishTypeAdminDto>> GetAllDishTypesAsync(bool includeDeleted = false)
        {
            var dishTypes = await _repository.GetAllAdminAsync(includeDeleted);

            var result = new List<DishTypeAdminDto>();
            foreach (var dishType in dishTypes)
            {
                var isUsed = await _repository.IsDishTypeUsedAsync(dishType.Id);
                result.Add(new DishTypeAdminDto
                {
                    Id = dishType.Id,
                    Name = dishType.Name,
                    IsDeleted = dishType.IsDeleted,
                    IsUsedInRecipes = isUsed
                });
            }

            return result;
        }

        public async Task<DishTypeAdminDto> GetDishTypeByIdAsync(int id)
        {
            var dishType = await _repository.GetByIdAdminAsync(id, true);
            if (dishType == null)
                throw new ApplicationException("Тип блюда не найден");

            var isUsed = await _repository.IsDishTypeUsedAsync(id);

            return new DishTypeAdminDto
            {
                Id = dishType.Id,
                Name = dishType.Name,
                IsDeleted = dishType.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<DishTypeAdminDto> CreateDishTypeAsync(CreateDishTypeDto dto)
        {
            var dishType = new DishType
            {
                Name = dto.Name,
                IsDeleted = false
            };

            var createdDishType = await _repository.AddAsync(dishType);

            return new DishTypeAdminDto
            {
                Id = createdDishType.Id,
                Name = createdDishType.Name,
                IsDeleted = createdDishType.IsDeleted,
                IsUsedInRecipes = false
            };
        }

        public async Task<DishTypeAdminDto> UpdateDishTypeAsync(UpdateDishTypeDto dto)
        {
            var dishType = await _repository.GetByIdAdminAsync(dto.Id, true);
            if (dishType == null)
                throw new ApplicationException("Тип блюда не найден");

            if (dishType.IsDeleted)
                throw new ApplicationException("Нельзя редактировать удаленный тип блюда");

            // Проверяем, используется ли тип блюда в рецептах
            var isUsed = await _repository.IsDishTypeUsedAsync(dto.Id);
            if (isUsed)
                throw new ApplicationException("Нельзя редактировать тип блюда, который используется в рецептах");

            dishType.Name = dto.Name;

            var updatedDishType = await _repository.UpdateAsync(dishType);

            return new DishTypeAdminDto
            {
                Id = updatedDishType.Id,
                Name = updatedDishType.Name,
                IsDeleted = updatedDishType.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<bool> DeleteDishTypeAsync(int id)
        {
            return await _repository.SoftDeleteAsync(id);
        }

        public async Task<bool> RestoreDishTypeAsync(int id)
        {
            return await _repository.RestoreAsync(id);
        }

        public async Task<bool> IsDishTypeUsedAsync(int id)
        {
            return await _repository.IsDishTypeUsedAsync(id);
        }
    }
}