using CookIt.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string ShortDescription { get; set; } = string.Empty; 
        [Required]
        public string FullDescription { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty; 
        public string OriginalImageUrl { get; set; } = string.Empty; 
        public string PreviewImageUrl { get; set; } = string.Empty; 

        [ForeignKey("DishType")]
        public int DishTypeId { get; set; }
        public DishType DishType { get; set; }

        public List<RecipeIngredientDto> RecipeIngredients { get; set; } = new();
        public List<RecipeEquipmentDto> RecipeEquipments { get; set; } = new();

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

        public double TotalCalories { get; set; }
        public double TotalProteins { get; set; }
        public double TotalFats { get; set; }
        public double TotalCarbohydrates { get; set; }

        public double CaloriesPerServing { get; set; }
        public double ProteinsPerServing { get; set; }
        public double FatsPerServing { get; set; }
        public double CarbohydratesPerServing { get; set; }

        public double CaloriesPer100g { get; set; }
        public double ProteinsPer100g { get; set; }
        public double FatsPer100g { get; set; }
        public double CarbohydratesPer100g { get; set; }

        public string CreatorId { get; set; }
        public string CreatorUsername { get; set; } 
        public string CreatorAvatarUrl { get; set; }
        public bool IsFavorite { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int? UserRating { get; set; }

        public bool? IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedById { get; set; }
        public string? RejectionComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ApprovedByUsername { get; set; }
        public string ModerationStatus
        {
            get
            {
                if (IsApproved == true) return "Одобрено";
                if (IsApproved == false) return "Отклонено";
                return "На модерации";
            }
        }

        public bool CanEdit => IsApproved != true; // Можно редактировать, если не одобрено
        public bool CanResubmit => IsApproved == false; // Можно отправить повторно, если отклонено
    }

}
