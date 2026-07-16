namespace CookIt.Core.Dtos.AI
{
    public class CommentModerationRequest
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
