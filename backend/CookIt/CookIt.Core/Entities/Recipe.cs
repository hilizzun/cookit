namespace CookIt.Core.Entities
{
    public class Recipe
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int CookingTimeWithUser { get; set; }
        public int CookingTimeWithoutUser { get; set; }
        public int SpicinessLevel { get; set; }
        public int DifficultyLevel { get; set; }
        public int DishTypeId { get; set; }
        public DishType DishType { get; set; }
        public string CreatorId { get; set; }
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

        public bool? IsApproved { get; set; } 
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedById { get; set; }
        public string? RejectionComment { get; set; }
        public ApplicationUser? ApprovedBy { get; set; }

        public DateTime? LastCommentsCheck { get; set; }

        public virtual List<RecipeIngredient> RecipeIngredients { get; set; } = new();

        public virtual List<RecipeEquipment> RecipeEquipments { get; set; } = new();
        public virtual ICollection<UserFavorite> FavoritedBy { get; set; } = new List<UserFavorite>();
        public virtual ICollection<RecipeRating> Ratings { get; set; } = new List<RecipeRating>();

    }
}