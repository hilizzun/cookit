using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class UpdateRecipeDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ShortDescription { get; set; } = string.Empty;
        [Required]
        public string FullDescription { get; set; } = string.Empty;

        public int DishTypeId { get; set; }

        public List<RecipeIngredientCreateDto> RecipeIngredients { get; set; } = new();
        public List<int> RecipeEquipments { get; set; } = new();

        [Range(1, int.MaxValue)]
        public int CookingTimeWithUser { get; set; }

        [Range(1, int.MaxValue)]
        public int CookingTimeWithoutUser { get; set; }

        [Range(1, 5)]
        public int DifficultyLevel { get; set; }

        [Range(1, 5)]
        public int SpicinessLevel { get; set; }

        [Range(1, 12)]
        public int? Servings { get; set; }
    }
}