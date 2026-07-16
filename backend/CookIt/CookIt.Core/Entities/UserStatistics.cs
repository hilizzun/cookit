namespace CookIt.Core.Entities
{
    public class UserStatistics
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public int RecipesPublished { get; set; }

        public int FavoritesAdded { get; set; }

        public int ShoppingListAdded { get; set; }
        public int CommentsLeft { get; set; }
        public int WheelSpins { get; set; }

        public int FiveStarRatingsReceived { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}