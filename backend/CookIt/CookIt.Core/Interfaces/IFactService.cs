using System.Collections.Generic;
using System.Threading.Tasks;

namespace CookIt.Core.Interfaces
{
    public interface IFactService
    {
        Task<IEnumerable<string>> GetFactsForRecipeAsync(int recipeId);
    }
}