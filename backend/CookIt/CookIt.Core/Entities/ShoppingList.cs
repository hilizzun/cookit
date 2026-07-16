namespace CookIt.Core.Entities
{
    public class ShoppingList
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int RecipeId { get; set; }
        public double Servings { get; set; }  
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Recipe Recipe { get; set; } = null!;
        public ICollection<ShoppingListExcludedIngredient> ExcludedIngredients { get; set; } = new List<ShoppingListExcludedIngredient>();
    }
}