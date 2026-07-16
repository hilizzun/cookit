namespace CookIt.Core.Dtos.Recipes
{
    public class RecipeModerationListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string DishTypeName { get; set; } = string.Empty;
    }
}
