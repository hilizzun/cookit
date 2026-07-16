using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class ModerateRecipeDto
    {
        [Required]
        public int RecipeId { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
        public string? RejectionComment { get; set; }
    }
}
