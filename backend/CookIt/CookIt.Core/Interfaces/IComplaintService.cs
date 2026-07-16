namespace CookIt.Core.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintDto> CreateComplaintAsync(int commentId, string userId, CreateComplaintDto dto);
        Task<IEnumerable<ComplaintDto>> GetPendingComplaintsAsync();
        Task ResolveComplaintAsync(int complaintId, string adminUserId, ResolveComplaintDto dto);
    }
}