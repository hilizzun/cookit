using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeCreateRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [Required]
        public string FullDescription { get; set; } = string.Empty;

        [Required]
        public int DishTypeId { get; set; }

        [Required]
        public List<int> RecipeEquipments { get; set; } = new();

        [Range(1, int.MaxValue)]
        public int CookingTimeWithUser { get; set; }

        [Range(1, int.MaxValue)]
        public int CookingTimeWithoutUser { get; set; }

        [Range(1, 5)]
        public int SpicinessLevel { get; set; }

        [Range(1, 5)]
        public int DifficultyLevel { get; set; }

        [Range(1, 12)]
        public int? Servings { get; set; }
        public List<int> IngredientIds { get; set; } = new();
        public List<double?> Quantities { get; set; } = new();
        public List<int?> UnitIds { get; set; } = new();

        public IFormFile? Image { get; set; }
    }
}