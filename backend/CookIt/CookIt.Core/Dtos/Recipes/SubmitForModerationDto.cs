using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class SubmitForModerationDto
    {
        [Required]
        public int RecipeId { get; set; }
    }
}
