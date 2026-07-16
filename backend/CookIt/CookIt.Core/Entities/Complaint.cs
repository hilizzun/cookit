namespace CookIt.Core.Entities
{
    public class Complaint
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public Comment Comment { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? Reason { get; set; }
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public string? ResolvedByUserId { get; set; }
        public ApplicationUser? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNote { get; set; }
    }

    public enum ComplaintStatus
    {
        Pending,
        Resolved,
        Rejected
    }
}