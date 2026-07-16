namespace CookIt.Core.Dtos.ShopingList
{
    public class ShoppingListIngredientDto
    {
        public int IngredientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsExcluded { get; set; }
        public bool IsByPiece { get; set; }               
        public double? ConversionToGrams { get; set; }
    }
}
