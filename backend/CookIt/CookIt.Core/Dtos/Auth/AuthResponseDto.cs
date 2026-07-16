public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public string RefreshToken { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; }
    public string Message { get; set; }
    public bool IsEmailVerified { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public bool IsBlocked { get; set; }
    public string BlockedReason { get; set; }
    public DateTime BlockedUntil { get; set; }

}
