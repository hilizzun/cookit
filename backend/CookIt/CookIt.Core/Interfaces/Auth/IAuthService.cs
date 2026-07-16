using CookIt.Core.Dtos.Auth;
using Microsoft.AspNetCore.Http;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, HttpResponse response);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, HttpResponse response);
    Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<AuthResponseDto> DeleteAccountAsync(string userId, DeleteAccountRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(HttpResponse response);
    Task LogoutAsync(string userId);
}
