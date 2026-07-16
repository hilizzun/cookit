using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeIngredientCreateDto
    {
        [Required]

        public int IngredientId { get; set; }
        public double? Quantity { get; set; }
        public int? UnitId { get; set; }
    }
}