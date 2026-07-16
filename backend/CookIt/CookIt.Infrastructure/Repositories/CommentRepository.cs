using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly DbContext _context;

        public CommentRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByRecipeIdAsync(int recipeId, int pageNumber, int pageSize)
        {
            return await _context.Set<Comment>()
                .Include(c => c.User) 
                .Where(c => c.RecipeId == recipeId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Set<Comment>()
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _context.Set<Comment>().AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Set<Comment>().Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteCommentAsync(int id)
        {
            var comment = await _context.Set<Comment>().FindAsync(id);
            if (comment != null)
            {
                comment.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUserCommentCountWithinTimeSpanAsync(string userId, TimeSpan timeSpan)
        {
            var since = DateTime.UtcNow - timeSpan;
            return await _context.Set<Comment>()
                .Where(c => c.UserId == userId && c.CreatedAt >= since)
                .CountAsync();
        }

        public async Task<IEnumerable<Comment>> GetNewCommentsForRecipeAsync(int recipeId, DateTime? lastCheck)
        {
            var query = _context.Set<Comment>().Where(c => c.RecipeId == recipeId && !c.IsDeleted);
            if (lastCheck.HasValue)
                query = query.Where(c => c.CreatedAt > lastCheck.Value);
            return await query.OrderBy(c => c.CreatedAt).ToListAsync();
        }
    }
}