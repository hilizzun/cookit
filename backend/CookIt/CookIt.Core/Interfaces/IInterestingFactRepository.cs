using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IInterestingFactRepository
    {
        Task<bool> HasFactsForEntityAsync(string entityType, int entityId);
        Task<List<InterestingFact>> GetFactsForEntityAsync(string entityType, int entityId);
        Task AddFactAsync(InterestingFact fact);
        Task SaveChangesAsync();
        Task<List<int>> GetRecipeIdsWithoutFactsAsync();
        Task<List<int>> GetIngredientIdsWithoutFactsAsync();
    }
}