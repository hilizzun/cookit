namespace CookIt.Core.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Calories { get; set; } 
        public double Proteins { get; set; } 
        public double Fats { get; set; } 
        public double Carbohydrates { get; set; } 
        public bool IsByPiece { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<RecipeIngredient> RecipeIngredients { get; set; } = new();
    }
}
