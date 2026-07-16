using CookIt.Core.Dtos.Auth;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using CookIt.Core.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CookIt.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMinioImageStorage _imageStorage;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            LinkGenerator linkGenerator,
            IMinioImageStorage imageStorage,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
            _linkGenerator = linkGenerator;
            _imageStorage = imageStorage;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, HttpResponse response)
        {
            _logger.LogInformation(
                "Начало регистрации пользователя. Email: {Email}, Username: {Username}",
                request.Email, request.Username);

            var userExists = await _userManager.FindByNameAsync(request.Username);
            if (userExists != null)
            {
                _logger.LogWarning(
                    "Попытка регистрации с существующим именем пользователя. Username: {Username}",
                    request.Username);
                throw new ApplicationException("Пользователь уже существует");
            }

            var emailExists = await _userManager.FindByEmailAsync(request.Email);
            if (emailExists != null)
            {
                _logger.LogWarning(
                    "Попытка регистрации с существующим email. Email: {Email}",
                    request.Email);
                throw new ApplicationException("Email уже используется");
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                FullName = request.FullName,
                EmailConfirmed = false
            };

            _logger.LogDebug(
                "Создание пользователя в базе данных. Email: {Email}, Username: {Username}",
                request.Email, request.Username);

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Ошибка при создании пользователя. Email: {Email}, Ошибки: {Errors}",
                    request.Email, string.Join("; ", result.Errors.Select(e => e.Description)));
                throw new ApplicationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation(
                "Пользователь создан успешно. UserId: {UserId}, Email: {Email}",
                user.Id, user.Email);

            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogDebug(
                "Пользователю {UserId} присвоена роль 'User'",
                user.Id);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontendBaseUrl = "http://localhost:5173";
            var confirmationLink = $"{frontendBaseUrl}/confirm-email/{WebUtility.UrlEncode(user.Id)}/{WebUtility.UrlEncode(token)}";

            var emailBody = $@"
                <h1 style='color: #201469; font-family: Arial, sans-serif;'>Добро пожаловать в CookIt!</h1>
    
                <p>Мы рады, что вы присоединились к нашему сообществу любителей вкусной еды!</p>
                <p>Чтобы начать пользоваться всеми возможностями сайта, пожалуйста, подтвердите ваш email:</p>
    
                <div style='margin: 30px 0;'>
                  <a href='{confirmationLink}' 
                     style='background-color: #201469; 
                            color: white; 
                            padding: 15px 30px; 
                            text-decoration: none; 
                            border-radius: 12px;
                            font-weight: bold;
                            display: inline-block;'>
                    ПОДТВЕРДИТЬ EMAIL
                  </a>
                </div>
                <h3>После подтверждения вы сможете:</h3>
                <ul>
                  <li>Сохранять любимые рецепты в избранное</li>
                  <li>Создавать собственные кулинарные шедевры</li>
                  <li>Быть в курсе всех самых свежих кулинарных идей</li>
                </ul>
    
                <p style='border-top: 1px solid #eee; padding-top: color: #777;'>
                  <em>Это вы?<br>
                  Если вы не регистрировались на CookIt, пожалуйста, проигнорируйте это письмо.</em>
                </p>
    
                <p>С любовью и аппетитными рецептами,<br>
                <strong>Команда CookIt</strong></p>
                </div>";

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Добро пожаловать в CookIt!",
                    emailBody);

                _logger.LogInformation(
                    "Письмо с подтверждением отправлено на email: {Email}",
                    user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при отправке письма подтверждения на email: {Email}",
                    user.Email);
                // Не выбрасываем исключение, чтобы не прерывать регистрацию
            }

            _logger.LogInformation(
                "Регистрация пользователя завершена успешно. UserId: {UserId}",
                user.Id);

            return new AuthResponseDto
            {
                Message = "Письмо с подтверждением отправлено на ваш email. Пожалуйста, проверьте почту."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, HttpResponse response)
        {
            _logger.LogDebug(
                "Попытка входа пользователя. Email: {Email}, IP: {RemoteIp}",
                request.Email, _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning(
                    "Неудачная попытка входа. Email: {Email}, Причина: Неверный логин или пароль",
                    request.Email);
                throw new ApplicationException("Неверный логин или пароль");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning(
                    "Попытка входа без подтвержденного email. UserId: {UserId}, Email: {Email}",
                    user.Id, user.Email);
                throw new ApplicationException("Подтвердите ваш email перед входом");
            }

            if (user.IsBlocked)
            {
                var reason = user.BlockedReason ?? "Причина не указана";
                var until = user.BlockedUntil.HasValue ?
                    $" до {user.BlockedUntil.Value.ToString("dd.MM.yyyy HH:mm")}" : "";
                _logger.LogWarning(
                    "Попытка входа заблокированным пользователем. UserId: {UserId}, Причина: {Reason}, Заблокирован до: {BlockedUntil}",
                    user.Id, reason, user.BlockedUntil);
                throw new ApplicationException($"Пользователь заблокирован{until}. Причина: {reason}");
            }

            _logger.LogInformation(
                "Успешная аутентификация пользователя. UserId: {UserId}, Email: {Email}",
                user.Id, user.Email);

            return await GenerateTokensAsync(user, response);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(HttpResponse response)
        {
            var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];
            _logger.LogDebug("Попытка обновления токена. RefreshToken присутствует: {HasRefreshToken}",
                !string.IsNullOrEmpty(refreshToken));

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh-токен отсутствует в cookies");
                throw new ApplicationException("Refresh-токен отсутствует");
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                _logger.LogWarning("Пользователь с таким refresh-токеном не найден");
                throw new ApplicationException("Недействительный или просроченный refresh-токен");
            }

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Refresh-токен просрочен. UserId: {UserId}, Время истечения: {ExpiryTime}",
                    user.Id, user.RefreshTokenExpiryTime);
                throw new ApplicationException("Недействительный или просроченный refresh-токен");
            }

            _logger.LogInformation(
                "Обновление токенов для пользователя. UserId: {UserId}",
                user.Id);

            return await GenerateTokensAsync(user, response);
        }

        private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user, HttpResponse response)
        {
            _logger.LogDebug("Генерация токенов для пользователя. UserId: {UserId}", user.Id);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError(
                    "Ошибка при обновлении refresh-токена пользователя. UserId: {UserId}, Ошибки: {Errors}",
                    user.Id, string.Join("; ", updateResult.Errors.Select(e => e.Description)));
                throw new ApplicationException("Ошибка обновления токенов");
            }

            response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            _logger.LogInformation(
                "Токены успешно сгенерированы. UserId: {UserId}, AccessToken истекает: {AccessTokenExpiration}, RefreshToken истекает: {RefreshTokenExpiration}",
                user.Id, token.ValidTo, user.RefreshTokenExpiryTime);

            return new AuthResponseDto
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                AccessTokenExpiration = token.ValidTo,
                RefreshToken = refreshToken,
                UserName = user.UserName,
                UserId = user.Id,
                IsEmailVerified = user.EmailConfirmed,
                Roles = userRoles.ToList()
            };
        }

        public async Task LogoutAsync(string userId)
        {
            _logger.LogInformation("Выход пользователя из системы. UserId: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при попытке выхода. UserId: {UserId}", userId);
                throw new Exception("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Ошибка при очистке refresh-токена пользователя. UserId: {UserId}, Ошибки: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                throw new Exception("Ошибка при выходе из системы");
            }

            _logger.LogInformation("Пользователь успешно вышел из системы. UserId: {UserId}", userId);
        }

        private string GenerateRefreshToken()
        {
            _logger.LogDebug("Генерация нового refresh-токена");
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<AuthResponseDto> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            _logger.LogInformation("Смена пароля пользователем. UserId: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при попытке смены пароля. UserId: {UserId}", userId);
                throw new ApplicationException("Пользователь не найден");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Ошибка при смене пароля пользователя. UserId: {UserId}, Ошибки: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                throw new ApplicationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation("Пароль успешно изменен. UserId: {UserId}", userId);

            return new AuthResponseDto
            {
                Message = "Пароль успешно изменен"
            };
        }

        public async Task<AuthResponseDto> DeleteAccountAsync(string userId, DeleteAccountRequestDto request)
        {
            _logger.LogWarning("Удаление аккаунта пользователем. UserId: {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при попытке удаления аккаунта. UserId: {UserId}", userId);
                throw new ApplicationException("Пользователь не найден");
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Неверный пароль при попытке удаления аккаунта. UserId: {UserId}", userId);
                throw new ApplicationException("Неверный пароль");
            }

            if (!string.IsNullOrEmpty(user.AvatarKey))
            {
                try
                {
                    _logger.LogDebug("Удаление аватарки пользователя. UserId: {UserId}, AvatarKey: {AvatarKey}",
                        userId, user.AvatarKey);
                    await _imageStorage.DeleteImageAsync(user.AvatarKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Ошибка при удалении аватарки пользователя. UserId: {UserId}, AvatarKey: {AvatarKey}",
                        userId, user.AvatarKey);
                    // Продолжаем удаление аккаунта даже если аватарка не удалилась
                }
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Ошибка при удалении аккаунта пользователя. UserId: {UserId}, Ошибки: {Errors}",
                    userId, string.Join("; ", result.Errors.Select(e => e.Description)));
                throw new ApplicationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            _logger.LogWarning("Аккаунт успешно удален. UserId: {UserId}", userId);

            return new AuthResponseDto
            {
                Message = "Аккаунт успешно удален"
            };
        }
    }
}