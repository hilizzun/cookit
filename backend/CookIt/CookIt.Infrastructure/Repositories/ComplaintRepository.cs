using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly DbContext _context;

        public ComplaintRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<Complaint?> GetByIdAsync(int id)
        {
            return await _context.Set<Complaint>()
                .Include(c => c.Comment)
                    .ThenInclude(cm => cm.Recipe)
                .Include(c => c.User)
                .Include(c => c.ResolvedBy)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Complaint>> GetPendingAsync()
        {
            return await _context.Set<Complaint>()
                .Include(c => c.Comment)
                .Include(c => c.User)
                .Where(c => c.Status == ComplaintStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Complaint?> GetByCommentAndUserAsync(int commentId, string userId)
        {
            return await _context.Set<Complaint>()
                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);
        }

        public async Task<Complaint> AddAsync(Complaint complaint)
        {
            await _context.Set<Complaint>().AddAsync(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task UpdateAsync(Complaint complaint)
        {
            _context.Set<Complaint>().Update(complaint);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserComplainedAsync(int commentId, string userId)
        {
            return await _context.Set<Complaint>()
                .AnyAsync(c => c.CommentId == commentId && c.UserId == userId);
        }
        public async Task<IEnumerable<Complaint>> GetByCommentIdAsync(int commentId)
        {
            return await _context.Set<Complaint>()
                .Where(c => c.CommentId == commentId && c.Status == ComplaintStatus.Pending)
                .ToListAsync();
        }

        public async Task<Complaint> CreateAutoComplaintAsync(int commentId, string reason)
        {
            var systemUser = await _context.Set<ApplicationUser>().FirstOrDefaultAsync(u => u.Id == "system");
            if (systemUser == null)
            {
                systemUser = new ApplicationUser
                {
                    Id = "system",
                    UserName = "System",
                    NormalizedUserName = "SYSTEM",
                    Email = "system@cookit.local",
                    NormalizedEmail = "SYSTEM@COOKIT.LOCAL",
                    EmailConfirmed = true,
                    FullName = "System User",   
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                await _context.Set<ApplicationUser>().AddAsync(systemUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                var entry = _context.Entry(systemUser);
                if (entry.State == EntityState.Detached)
                {
                    _context.Set<ApplicationUser>().Attach(systemUser);
                }
            }

            var complaint = new Complaint
            {
                CommentId = commentId,
                UserId = systemUser.Id,
                Reason = reason,
                Status = ComplaintStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Set<Complaint>().AddAsync(complaint);
            await _context.SaveChangesAsync();
            return complaint;
        }
    }
}