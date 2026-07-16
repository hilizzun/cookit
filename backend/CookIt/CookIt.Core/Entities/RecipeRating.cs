namespace CookIt.Core.Entities
{
    public class RecipeRating
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public int Value { get; set; }
        public DateTime RatedAt { get; set; } = DateTime.UtcNow;
    }
}