using CookIt.Core.Dtos.Auth;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CookIt.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly Core.Interfaces.IEmailSender _emailSender;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            Core.Interfaces.IEmailSender emailSender,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            _logger.LogDebug("Начало получения всех пользователей");

            try
            {
                var users = _userManager.Users.ToList();
                var result = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    result.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = roles
                    });
                }

                _logger.LogInformation("Успешно получено {UserCount} пользователей", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех пользователей");
                throw;
            }
        }

        public async Task<(bool Success, LoginResponseDto? Data, string ErrorMessage)> LoginAsync(LoginRequestDto request)
        {
            _logger.LogDebug("Попытка входа пользователя через UserService. Email: {Email}", request.Email);

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Неудачная попытка входа через UserService. Email: {Email}, Причина: Неверный Email или пароль",
                    request.Email);
                return (false, null, "Неверный Email или пароль.");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Попытка входа без подтвержденного email через UserService. Email: {Email}",
                    request.Email);
                return (false, null, "Подтвердите ваш email перед входом");
            }

            try
            {
                var token = await GenerateJwtToken(user);
                var response = new LoginResponseDto
                {
                    AccessToken = token,
                    UserName = user.UserName!,
                    Email = user.Email!
                };

                _logger.LogInformation("Успешный вход через UserService. Email: {Email}", request.Email);
                return (true, response, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации JWT токена для пользователя. Email: {Email}", request.Email);
                return (false, null, "Ошибка при аутентификации");
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogDebug("Генерация JWT токена для пользователя. UserId: {UserId}", user.Id);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            _logger.LogDebug("JWT токен сгенерирован для пользователя. UserId: {UserId}, Истекает: {Expiration}",
                user.Id, DateTime.UtcNow.AddMinutes(expirationMinutes));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDto> BlockUserAsync(string userId, string reason, DateTime? blockUntil = null)
        {
            using (_logger.BeginScope(new { UserId = userId, Reason = reason, BlockUntil = blockUntil }))
            {
                _logger.LogWarning("Начало блокировки пользователя");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при попытке блокировки. UserId: {UserId}", userId);
                    throw new ApplicationException("Пользователь не найден");
                }

                user.IsBlocked = true;
                user.BlockedReason = reason;
                user.BlockedUntil = blockUntil;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Ошибка при блокировке пользователя. UserId: {UserId}, Ошибки: {Errors}",
                        userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                    throw new ApplicationException("Ошибка при блокировке пользователя");
                }

                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogWarning(
                    "Пользователь успешно заблокирован. UserId: {UserId}, Причина: {Reason}, Заблокирован до: {BlockUntil}",
                    userId, reason, blockUntil);

                return new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                    IsBlocked = user.IsBlocked,
                    BlockedReason = user.BlockedReason,
                    BlockedUntil = user.BlockedUntil
                };
            }
        }

        public async Task<UserDto> UnblockUserAsync(string userId)
        {
            using (_logger.BeginScope(new { UserId = userId }))
            {
                _logger.LogInformation("Начало разблокировки пользователя");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при попытке разблокировки. UserId: {UserId}", userId);
                    throw new ApplicationException("Пользователь не найден");
                }

                user.IsBlocked = false;
                user.BlockedReason = null;
                user.BlockedUntil = null;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Ошибка при разблокировке пользователя. UserId: {UserId}, Ошибки: {Errors}",
                        userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                    throw new ApplicationException("Ошибка при разблокировке пользователя");
                }

                var roles = await _userManager.GetRolesAsync(user);

                _logger.LogInformation("Пользователь успешно разблокирован. UserId: {UserId}", userId);

                return new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                    IsBlocked = user.IsBlocked,
                    BlockedReason = user.BlockedReason,
                    BlockedUntil = user.BlockedUntil
                };
            }
        }

        public async Task<List<UserDto>> GetAllUsersWithDetailsAsync()
        {
            _logger.LogDebug("Начало получения всех пользователей с деталями");

            try
            {
                var users = _userManager.Users
                    .Include(u => u.Favorites)
                    .Include(u => u.Ratings)
                    .ToList();

                var result = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    result.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        Roles = roles,
                        IsBlocked = user.IsBlocked,
                        BlockedReason = user.BlockedReason,
                        BlockedUntil = user.BlockedUntil
                    });
                }

                _logger.LogInformation("Успешно получено {UserCount} пользователей с деталями", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех пользователей с деталями");
                throw;
            }
        }

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles,
                IsBlocked = user.IsBlocked,
                BlockedReason = user.BlockedReason,
                BlockedUntil = user.BlockedUntil,
                AvatarKey = user.AvatarKey
            };
        }
    }
}