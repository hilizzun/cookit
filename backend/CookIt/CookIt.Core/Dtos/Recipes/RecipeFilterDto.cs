using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeFilterDto
    {
        public int? MinCookingTimeWithUser { get; set; }
        public int? MaxCookingTimeWithUser { get; set; }
        public int? MinCookingTimeWithoutUser { get; set; }
        public int? MaxCookingTimeWithoutUser { get; set; }

        public int? MinSpicinessLevel { get; set; }
        public int? MaxSpicinessLevel { get; set; }

        public int? MinDifficultyLevel { get; set; }
        public int? MaxDifficultyLevel { get; set; }

        public List<int> DishTypeIds { get; set; } = new List<int>();
        public List<int> EquipmentIds { get; set; } = new List<int>();
        public List<int> IngredientIds { get; set; } = new List<int>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool? IsApproved { get; set; }

        public string? SearchText { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinCookingTimeWithUser.HasValue && MaxCookingTimeWithUser.HasValue &&
                MinCookingTimeWithUser > MaxCookingTimeWithUser)
            {
                yield return new ValidationResult("Максимальное время должно быть больше минимального");
            }

            if (MinCookingTimeWithoutUser.HasValue && MaxCookingTimeWithoutUser.HasValue &&
                MinCookingTimeWithoutUser > MaxCookingTimeWithoutUser)
            {
                yield return new ValidationResult("Максимальное время должно быть больше минимального");
            }
            if (MinSpicinessLevel.HasValue && (MinSpicinessLevel < 0 || MinSpicinessLevel > 5))
            {
                yield return new ValidationResult("Уровень остроты должен быть от 0 до 5");
            }

            if (MaxSpicinessLevel.HasValue && (MaxSpicinessLevel < 0 || MaxSpicinessLevel > 5))
            {
                yield return new ValidationResult("Уровень остроты должен быть от 0 до 5");
            }

            if (MinDifficultyLevel.HasValue && (MinDifficultyLevel < 1 || MinDifficultyLevel > 5))
            {
                yield return new ValidationResult("Уровень сложности должен быть от 1 до 5");
            }

            if (MaxDifficultyLevel.HasValue && (MaxDifficultyLevel < 1 || MaxDifficultyLevel > 5))
            {
                yield return new ValidationResult("Уровень сложности должен быть от 1 до 5");
            }
        }
    }
}