using CookIt.Core.Interfaces;

namespace CookIt.Application.Services
{
    public class FactService : IFactService
    {
        private readonly IInterestingFactRepository _factRepository;
        private readonly IRecipeRepository _recipeRepository;

        public FactService(IInterestingFactRepository factRepository, IRecipeRepository recipeRepository)
        {
            _factRepository = factRepository;
            _recipeRepository = recipeRepository;
        }

        public async Task<IEnumerable<string>> GetFactsForRecipeAsync(int recipeId)
        {
            var recipeFacts = await _factRepository.GetFactsForEntityAsync("Recipe", recipeId);
            var recipeFactTexts = recipeFacts.Select(f => f.FactText);

            var ingredientIds = await _recipeRepository.GetIngredientIdsByRecipeIdAsync(recipeId);
            var ingredientFactTexts = new List<string>();

            foreach (var ingredientId in ingredientIds)
            {
                var ingredientFacts = await _factRepository.GetFactsForEntityAsync("Ingredient", ingredientId);
                ingredientFactTexts.AddRange(ingredientFacts.Select(f => f.FactText));
            }

            var allFacts = recipeFactTexts.Concat(ingredientFactTexts).Distinct();
            return allFacts;
        }
    }
}