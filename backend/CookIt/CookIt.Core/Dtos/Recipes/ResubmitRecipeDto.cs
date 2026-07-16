using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class ResubmitRecipeDto
    {
        [Required]
        public int RecipeId { get; set; }
    }
}
