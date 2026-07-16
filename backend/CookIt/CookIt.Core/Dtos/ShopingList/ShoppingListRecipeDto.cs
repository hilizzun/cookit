namespace CookIt.Core.Dtos.ShopingList
{
    public class ShoppingListRecipeDto
    {
        public int Id { get; set; }               
        public int RecipeId { get; set; }
        public string RecipeName { get; set; } = string.Empty;
        public double Servings { get; set; }
        public double OriginalServings { get; set; }  
        public List<ShoppingListIngredientDto> Ingredients { get; set; } = new();
    }
}
