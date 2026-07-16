public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public int RecipeId { get; set; }
    public int? ParentCommentId { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
    public bool IsDeleted { get; set; }
}