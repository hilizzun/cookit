namespace CookIt.Core.Entities
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public double? Quantity { get; set; }
        public int? UnitId { get; set; }
        public Unit Unit { get; set; }
    }
}
