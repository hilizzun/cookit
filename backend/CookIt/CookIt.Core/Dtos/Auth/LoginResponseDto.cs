namespace CookIt.Core.Dtos.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = default!;
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
