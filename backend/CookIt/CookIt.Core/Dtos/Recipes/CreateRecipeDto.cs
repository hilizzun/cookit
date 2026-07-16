using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class CreateRecipeDto
    {
        [Required]

        public string Name { get; set; } = string.Empty;
        [Required]
        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty; 
        [Required]
        public string FullDescription { get; set; } = string.Empty;
        [Required]

        public int DishTypeId { get; set; }
        [Required]

        public List<RecipeIngredientCreateDto> RecipeIngredients { get; set; } = new();

        public List<int> RecipeEquipments { get; set; } = new();

        public int CookingTimeWithUser { get; set; }

        public int CookingTimeWithoutUser { get; set; }

        public int SpicinessLevel { get; set; }

        public int DifficultyLevel { get; set; }

        [Range(1, 12)]
        public int? Servings { get; set; }
    }
}
