namespace CookIt.Core.Dtos.Auth
{
    public class RegisterRequestDto
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FullName { get; set; }
    }
}
