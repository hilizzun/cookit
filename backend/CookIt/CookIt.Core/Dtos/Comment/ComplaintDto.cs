public class ComplaintDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public string CommentContent { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResolvedByUserName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
    public int RecipeId { get; set; }

}