using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class InterestingFactRepository : IInterestingFactRepository
    {
        private readonly CookItContext _context;

        public InterestingFactRepository(CookItContext context)
        {
            _context = context;
        }

        public async Task<bool> HasFactsForEntityAsync(string entityType, int entityId)
        {
            return await _context.InterestingFacts
                .AnyAsync(f => f.EntityType == entityType && f.EntityId == entityId);
        }

        public async Task<List<InterestingFact>> GetFactsForEntityAsync(string entityType, int entityId)
        {
            return await _context.InterestingFacts
                .Where(f => f.EntityType == entityType && f.EntityId == entityId)
                .ToListAsync();
        }

        public async Task AddFactAsync(InterestingFact fact)
        {
            await _context.InterestingFacts.AddAsync(fact);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetRecipeIdsWithoutFactsAsync()
        {
            var recipeIdsWithFacts = await _context.InterestingFacts
                .Where(f => f.EntityType == "Recipe")
                .Select(f => f.EntityId)
                .Distinct()
                .ToListAsync();

            var allRecipeIds = await _context.Recipes
                .Select(r => r.Id)
                .ToListAsync();

            return allRecipeIds.Except(recipeIdsWithFacts).ToList();
        }

        public async Task<List<int>> GetIngredientIdsWithoutFactsAsync()
        {
            var ingredientIdsWithFacts = await _context.InterestingFacts
                .Where(f => f.EntityType == "Ingredient")
                .Select(f => f.EntityId)
                .Distinct()
                .ToListAsync();

            var allIngredientIds = await _context.Ingredients
                .Select(i => i.Id)
                .ToListAsync();

            return allIngredientIds.Except(ingredientIdsWithFacts).ToList();
        }


    }
}