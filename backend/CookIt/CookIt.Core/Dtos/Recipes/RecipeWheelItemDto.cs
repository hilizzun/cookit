namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeWheelItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PreviewImageUrl { get; set; }
    }
}