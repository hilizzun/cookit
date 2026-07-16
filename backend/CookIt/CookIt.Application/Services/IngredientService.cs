using CookIt.Core.Entities;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _repository;

        public IngredientService(IIngredientRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Ingredient>> GetAllIngredientsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<Ingredient>> SearchIngredientsAsync(string searchText)
        {
            return await _repository.SearchAsync(searchText);
        }
    }
}