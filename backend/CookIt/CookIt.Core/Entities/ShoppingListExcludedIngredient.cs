namespace CookIt.Core.Entities
{
    public class ShoppingListExcludedIngredient
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }
        public int IngredientId { get; set; }

        public ShoppingList ShoppingList { get; set; } = null!;
        public Ingredient Ingredient { get; set; } = null!;
    }
}