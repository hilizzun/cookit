namespace CookIt.Core.Dtos.Auth
{
    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
