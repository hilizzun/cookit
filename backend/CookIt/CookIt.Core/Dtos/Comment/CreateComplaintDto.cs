using System.ComponentModel.DataAnnotations;

public class CreateComplaintDto
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}