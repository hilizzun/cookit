using CookIt.Core.Entities;

namespace CookIt.Core.Interfaces
{
    public interface IComplaintRepository
    {
        Task<Complaint?> GetByIdAsync(int id);
        Task<IEnumerable<Complaint>> GetPendingAsync();
        Task<Complaint?> GetByCommentAndUserAsync(int commentId, string userId);
        Task<Complaint> AddAsync(Complaint complaint);
        Task UpdateAsync(Complaint complaint);
        Task<bool> HasUserComplainedAsync(int commentId, string userId);
        Task<IEnumerable<Complaint>> GetByCommentIdAsync(int commentId);
        Task<Complaint> CreateAutoComplaintAsync(int commentId, string reason);
    }
}