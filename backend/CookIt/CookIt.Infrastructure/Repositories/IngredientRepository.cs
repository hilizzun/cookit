using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly DbContext _context;

        public IngredientRepository(DbContext context)
        {
            _context = context;
        }

        // Для обычных пользователей - только не удаленные
        public async Task<IEnumerable<Ingredient>> GetAllAsync()
        {
            return await _context.Set<Ingredient>()
                .Where(i => !i.IsDeleted)
                .ToListAsync();
        }

        // Для админ-панели - все с возможностью фильтрации
        public async Task<IEnumerable<Ingredient>> GetAllAdminAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Ingredient>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(i => !i.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Ingredient>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await GetAllAsync();
            }

            var query = _context.Set<Ingredient>()
                .Where(i => !i.IsDeleted)
                .AsQueryable();

            query = query.Where(i =>
                EF.Functions.ILike(i.Name, $"%{searchText}%")
            );

            return await query
                .OrderBy(i => i.Name)
                .Take(50)
                .ToListAsync();
        }

        public async Task<Ingredient?> GetByIdAsync(int id)
        {
            return await _context.Set<Ingredient>()
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<Ingredient?> GetByIdAdminAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Set<Ingredient>().Where(i => i.Id == id);

            if (!includeDeleted)
            {
                query = query.Where(i => !i.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Ingredient> AddAsync(Ingredient ingredient)
        {
            await _context.Set<Ingredient>().AddAsync(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<Ingredient> UpdateAsync(Ingredient ingredient)
        {
            _context.Set<Ingredient>().Update(ingredient);
            await _context.SaveChangesAsync();
            return ingredient;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var ingredient = await GetByIdAdminAsync(id, true);
            if (ingredient == null)
                return false;

            ingredient.IsDeleted = true;
            await UpdateAsync(ingredient);
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var ingredient = await GetByIdAdminAsync(id, true);
            if (ingredient == null)
                return false;

            ingredient.IsDeleted = false;
            await UpdateAsync(ingredient);
            return true;
        }

        public async Task<bool> IsIngredientUsedAsync(int ingredientId)
        {
            return await _context.Set<RecipeIngredient>()
                .AnyAsync(re => re.IngredientId == ingredientId);
        }
    }
}