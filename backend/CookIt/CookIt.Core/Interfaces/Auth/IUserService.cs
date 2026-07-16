using CookIt.Core.Dtos.Auth;

namespace CookIt.Core.Interfaces.Auth
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersWithDetailsAsync();
        Task<UserDto> BlockUserAsync(string userId, string reason, DateTime? blockUntil = null);
        Task<UserDto> UnblockUserAsync(string userId);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> GetUserByIdAsync(string userId);
        Task<(bool Success, LoginResponseDto? Data, string ErrorMessage)> LoginAsync(LoginRequestDto request);

    }
}