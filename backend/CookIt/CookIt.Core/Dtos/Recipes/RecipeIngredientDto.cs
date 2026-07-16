namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeIngredientDto
    {
        public int IngredientId { get; set; }
        public string? IngredientName { get; set; }
        public double? Quantity { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
    }
}
