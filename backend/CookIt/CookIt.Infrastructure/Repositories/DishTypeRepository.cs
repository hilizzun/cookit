using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class DishTypeRepository : IDishTypeRepository
    {
        private readonly DbContext _context;

        public DishTypeRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DishType>> GetAllAsync()
        {
            return await _context.Set<DishType>()
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<DishType>> GetAllAdminAsync(bool includeDeleted = false)
        {
            var query = _context.Set<DishType>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<DishType?> GetByIdAsync(int id)
        {
            return await _context.Set<DishType>()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<DishType?> GetByIdAdminAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Set<DishType>().Where(e => e.Id == id);

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<DishType> AddAsync(DishType dishType)
        {
            await _context.Set<DishType>().AddAsync(dishType);
            await _context.SaveChangesAsync();
            return dishType;
        }

        public async Task<DishType> UpdateAsync(DishType dishType)
        {
            _context.Set<DishType>().Update(dishType);
            await _context.SaveChangesAsync();
            return dishType;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var dishType = await GetByIdAdminAsync(id, true);
            if (dishType == null)
                return false;

            dishType.IsDeleted = true;
            await UpdateAsync(dishType);
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var dishType = await GetByIdAdminAsync(id, true);
            if (dishType == null)
                return false;

            dishType.IsDeleted = false;
            await UpdateAsync(dishType);
            return true;
        }

        public async Task<bool> IsDishTypeUsedAsync(int dishTypeId)
        {
            return await _context.Set<Recipe>()
                .AnyAsync(r => r.DishTypeId == dishTypeId);
        }
    }
}