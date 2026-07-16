using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly DbContext _context;

        public EquipmentRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Equipment>> GetAllAsync()
        {
            return await _context.Set<Equipment>()
                .Where(e => !e.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Equipment>> GetAllAdminAsync(bool includeDeleted = false)
        {
            var query = _context.Set<Equipment>().AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.ToListAsync();
        }

        public async Task<Equipment?> GetByIdAsync(int id)
        {
            return await _context.Set<Equipment>()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<Equipment?> GetByIdAdminAsync(int id, bool includeDeleted = false)
        {
            var query = _context.Set<Equipment>().Where(e => e.Id == id);

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Equipment> AddAsync(Equipment equipment)
        {
            await _context.Set<Equipment>().AddAsync(equipment);
            await _context.SaveChangesAsync();
            return equipment;
        }

        public async Task<Equipment> UpdateAsync(Equipment equipment)
        {
            _context.Set<Equipment>().Update(equipment);
            await _context.SaveChangesAsync();
            return equipment;
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var equipment = await GetByIdAdminAsync(id, true);
            if (equipment == null)
                return false;

            equipment.IsDeleted = true;
            await UpdateAsync(equipment);
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var equipment = await GetByIdAdminAsync(id, true);
            if (equipment == null)
                return false;

            equipment.IsDeleted = false;
            await UpdateAsync(equipment);
            return true;
        }

        public async Task<bool> IsEquipmentUsedAsync(int equipmentId)
        {
            return await _context.Set<RecipeEquipment>()
                .AnyAsync(re => re.EquipmentId == equipmentId);
        }
    }
}