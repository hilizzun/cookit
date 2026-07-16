using System.ComponentModel.DataAnnotations;

public class UpdateCommentDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}