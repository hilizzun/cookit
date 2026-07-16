using CookIt.Core.Dtos.Admin;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces.Admin;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services.Admin
{
    public class AdminIngredientService : IAdminIngredientService
    {
        private readonly IIngredientRepository _repository;

        public AdminIngredientService(IIngredientRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<IngredientAdminDto>> GetAllIngredientsAsync(bool includeDeleted = false)
        {
            var ingredients = await _repository.GetAllAdminAsync(includeDeleted);

            var result = new List<IngredientAdminDto>();
            foreach (var ingredient in ingredients)
            {
                var isUsed = await _repository.IsIngredientUsedAsync(ingredient.Id);
                result.Add(new IngredientAdminDto
                {
                    Id = ingredient.Id,
                    Name = ingredient.Name,
                    Calories = ingredient.Calories,
                    Proteins = ingredient.Proteins,
                    Fats = ingredient.Fats,
                    Carbohydrates = ingredient.Carbohydrates,
                    IsByPiece = ingredient.IsByPiece,
                    IsDeleted = ingredient.IsDeleted,
                    IsUsedInRecipes = isUsed
                });
            }

            return result;
        }

        public async Task<IngredientAdminDto> GetIngredientByIdAsync(int id)
        {
            var ingredient = await _repository.GetByIdAdminAsync(id, true);
            if (ingredient == null)
                throw new ApplicationException("Ингредиент не найден");

            var isUsed = await _repository.IsIngredientUsedAsync(id);

            return new IngredientAdminDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Calories = ingredient.Calories,
                Proteins = ingredient.Proteins,
                Fats = ingredient.Fats,
                Carbohydrates = ingredient.Carbohydrates,
                IsByPiece = ingredient.IsByPiece,
                IsDeleted = ingredient.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<IngredientAdminDto> CreateIngredientAsync(CreateIngredientDto dto)
        {
            var ingredient = new Ingredient
            {
                Name = dto.Name,
                Calories = dto.Calories,
                Proteins = dto.Proteins,
                Fats = dto.Fats,
                Carbohydrates = dto.Carbohydrates,
                IsByPiece = dto.IsByPiece,
                IsDeleted = false
            };

            var createdIngredient = await _repository.AddAsync(ingredient);

            return new IngredientAdminDto
            {
                Id = createdIngredient.Id,
                Name = createdIngredient.Name,
                Calories = createdIngredient.Calories,
                Proteins = createdIngredient.Proteins,
                Fats = createdIngredient.Fats,
                Carbohydrates = createdIngredient.Carbohydrates,
                IsByPiece = createdIngredient.IsByPiece,
                IsDeleted = createdIngredient.IsDeleted,
                IsUsedInRecipes = false
            };
        }

        public async Task<IngredientAdminDto> UpdateIngredientAsync(UpdateIngredientDto dto)
        {
            var ingredient = await _repository.GetByIdAdminAsync(dto.Id, true);
            if (ingredient == null)
                throw new ApplicationException("Ингредиент не найден");

            if (ingredient.IsDeleted)
                throw new ApplicationException("Нельзя редактировать удаленный ингредиент");

            // Проверяем, используется ли ингредиент в рецептах
            var isUsed = await _repository.IsIngredientUsedAsync(dto.Id);
            if (isUsed)
                throw new ApplicationException("Нельзя редактировать ингредиент, который используется в рецептах");

            ingredient.Name = dto.Name;
            ingredient.Calories = dto.Calories;
            ingredient.Proteins = dto.Proteins;
            ingredient.Fats = dto.Fats;
            ingredient.Carbohydrates = dto.Carbohydrates;
            ingredient.IsByPiece = dto.IsByPiece;

            var updatedIngredient = await _repository.UpdateAsync(ingredient);

            return new IngredientAdminDto
            {
                Id = updatedIngredient.Id,
                Name = updatedIngredient.Name,
                Calories = updatedIngredient.Calories,
                Proteins = updatedIngredient.Proteins,
                Fats = updatedIngredient.Fats,
                Carbohydrates = updatedIngredient.Carbohydrates,
                IsByPiece = updatedIngredient.IsByPiece,
                IsDeleted = updatedIngredient.IsDeleted,
                IsUsedInRecipes = isUsed
            };
        }

        public async Task<bool> DeleteIngredientAsync(int id)
        {
            return await _repository.SoftDeleteAsync(id);
        }

        public async Task<bool> RestoreIngredientAsync(int id)
        {
            return await _repository.RestoreAsync(id);
        }

        public async Task<bool> IsIngredientUsedAsync(int id)
        {
            return await _repository.IsIngredientUsedAsync(id);
        }
    }
}