using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unit = CookIt.Core.Entities.Unit;

namespace CookIt.Infrastructure.Repositories
{
    public class UnitRepository : IUnitRepository
    {
        private readonly DbContext _context;

        public UnitRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Unit>> GetAllAsync()
        {
            return await _context.Set<Unit>()
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Unit>> GetAllAdminAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Unit>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<Unit?> GetByIdAsync(int id)
        {
            return await _context.Set<Unit>()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<Unit?> GetByIdAdminAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Set<Unit>().Where(e => e.Id == id);

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Unit> AddAsync(Unit dishType)
        {
            await _context.Set<Unit>().AddAsync(dishType);
            await _context.SaveChangesAsync();
            return dishType;
        }

        public async Task<Unit> UpdateAsync(Unit dishType)
        {
            _context.Set<Unit>().Update(dishType);
            await _context.SaveChangesAsync();
            return dishType;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var unit = await GetByIdAdminAsync(id, true);
            if (unit == null)
                return false;

            unit.IsDeleted = true;
            await UpdateAsync(unit);
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var unit = await GetByIdAdminAsync(id, true);
            if (unit == null)
                return false;

            unit.IsDeleted = false;
            await UpdateAsync(unit);
            return true;
        }

        public async Task<bool> IsUnitUsedAsync(int unitId)
        {
            return await _context.Set<RecipeIngredient>()
                .AnyAsync(ri => ri.UnitId == unitId);
        }
    }
}