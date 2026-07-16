namespace CookIt.Core.Dtos.Auth
{
    public class UserDto
    {
        public string Id { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsBlocked { get; set; }
        public string? BlockedReason { get; set; }
        public DateTime? BlockedUntil { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? AvatarKey { get; set; }
    }
}