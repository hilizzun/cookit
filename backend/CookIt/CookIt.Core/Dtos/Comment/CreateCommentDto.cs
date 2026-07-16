using System.ComponentModel.DataAnnotations;

public class CreateCommentDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Комментарий не может быть пустым")]
    [MaxLength(1000, ErrorMessage = "Комментарий не может превышать 1000 символов")]
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}