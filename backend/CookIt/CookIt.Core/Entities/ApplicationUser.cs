using Microsoft.AspNetCore.Identity;

namespace CookIt.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string FullName { get; set; } = default!;
        public string? AvatarKey { get; set; }
        public bool IsBlocked { get; set; } = false;
        public string? BlockedReason { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public virtual ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
        public virtual ICollection<RecipeRating> Ratings { get; set; } = new List<RecipeRating>();
    }
}