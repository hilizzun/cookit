using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetCommentsByRecipeIdAsync(int recipeId, int pageNumber, int pageSize);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<Comment> AddCommentAsync(Comment comment);
        Task UpdateCommentAsync(Comment comment);
        Task SoftDeleteCommentAsync(int id);
        Task<int> GetUserCommentCountWithinTimeSpanAsync(string userId, TimeSpan timeSpan);
        Task<IEnumerable<Comment>> GetNewCommentsForRecipeAsync(int recipeId, DateTime? lastCheck);
    }
}