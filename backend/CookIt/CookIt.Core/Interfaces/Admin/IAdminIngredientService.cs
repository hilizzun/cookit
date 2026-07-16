using CookIt.Core.Dtos.Admin;

namespace CookIt.Core.Interfaces.Admin
{
    public interface IAdminIngredientService
    {
        Task<List<IngredientAdminDto>> GetAllIngredientsAsync(bool includeDeleted = false);
        Task<IngredientAdminDto> GetIngredientByIdAsync(int id);
        Task<IngredientAdminDto> CreateIngredientAsync(CreateIngredientDto dto);
        Task<IngredientAdminDto> UpdateIngredientAsync(UpdateIngredientDto dto);
        Task<bool> DeleteIngredientAsync(int id);
        Task<bool> RestoreIngredientAsync(int id);
        Task<bool> IsIngredientUsedAsync(int id);
    }
}