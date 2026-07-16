using System.ComponentModel.DataAnnotations;

namespace CookIt.Core.Dtos.Admin
{
    public class IngredientAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
        public bool IsByPiece { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUsedInRecipes { get; set; }
    }

    public class CreateIngredientDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
        public bool IsByPiece { get; set; }
    }

    public class UpdateIngredientDto : CreateIngredientDto
    {
        [Required]
        public int Id { get; set; }
    }
}