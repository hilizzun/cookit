using CookIt.Core.Dtos.Auth;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace CookIt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            _logger.LogInformation(
                "Попытка входа пользователя. Email: {Email}, IP: {RemoteIp}",
                request.Email, HttpContext.Connection.RemoteIpAddress);

            try
            {
                var result = await _authService.LoginAsync(request, Response);

                _logger.LogInformation(
                    "Успешный вход пользователя. UserId: {UserId}, Email: {Email}",
                    result.UserId, request.Email);

                return Ok(new
                {
                    accessToken = result.AccessToken,
                    userName = result.UserName,
                    userId = result.UserId,
                    isEmailVerified = result.IsEmailVerified,
                    roles = result.Roles
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Неудачная попытка входа. Email: {Email}, Ошибка: {ErrorMessage}",
                    request.Email, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            _logger.LogInformation(
                "Регистрация нового пользователя. Email: {Email}, Имя пользователя: {UserName}",
                request.Email, request.Username);

            try
            {
                var result = await _authService.RegisterAsync(request, Response);

                _logger.LogInformation(
                    "Пользователь успешно зарегистрирован. UserId: {UserId}, Email: {Email}",
                    result.UserId, request.Email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при регистрации пользователя. Email: {Email}, Имя: {UserName}",
                    request.Email, request.Username);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            _logger.LogDebug("Подтверждение email. UserId: {UserId}", userId);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Неверные параметры для подтверждения email");
                return BadRequest(new { message = "Неверные параметры" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при подтверждении email. UserId: {UserId}", userId);
                return NotFound(new { message = "Пользователь не найден" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Ошибка подтверждения email. UserId: {UserId}, Ошибки: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(new { message = "Ошибка подтверждения email" });
            }

            _logger.LogInformation("Email успешно подтвержден. UserId: {UserId}, Email: {Email}",
                userId, user.Email);

            return Ok(new
            {
                message = "Email успешно подтвержден",
                redirectPath = "/email-confirmed"
            });
        }

        [HttpGet("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            _logger.LogInformation("Повторная отправка подтверждения email. Email: {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден при повторной отправке подтверждения. Email: {Email}", email);
                return NotFound(new { message = "Пользователь не найден" });
            }

            if (user.EmailConfirmed)
            {
                _logger.LogWarning("Email уже подтвержден. Email: {Email}", email);
                return BadRequest(new { message = "Email уже подтвержден" });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontendBaseUrl = "http://localhost:5173";
            var confirmationLink = $"{frontendBaseUrl}/confirm-email/{WebUtility.UrlEncode(user.Id)}/{WebUtility.UrlEncode(token)}";

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Подтвердите ваш email для CookIt",
                    $"Пожалуйста, подтвердите ваш аккаунт: <a href='{confirmationLink}'>Подтвердить email</a>");

                _logger.LogInformation("Письмо подтверждения отправлено. Email: {Email}", email);
                return Ok(new { message = "Письмо отправлено повторно" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки письма подтверждения. Email: {Email}", email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Попытка выхода без аутентификации");
                return Unauthorized();
            }

            _logger.LogInformation("Выход пользователя. UserId: {UserId}", userId);

            try
            {
                await _authService.LogoutAsync(userId);
                Response.Cookies.Delete("refreshToken");

                _logger.LogInformation("Пользователь успешно вышел. UserId: {UserId}", userId);
                return Ok(new { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выходе пользователя. UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            _logger.LogDebug("Обновление токена");

            try
            {
                var result = await _authService.RefreshTokenAsync(Response);

                _logger.LogInformation("Токен успешно обновлен. UserId: {UserId}", result.UserId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Невалидный refresh token. Ошибка: {ErrorMessage}", ex.Message);
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении токена");
                return StatusCode(500, "Что-то пошло не так при обновлении токена");
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Попытка смены пароля без аутентификации");
                return Unauthorized();
            }

            _logger.LogInformation("Смена пароля пользователем. UserId: {UserId}", userId);

            try
            {
                var result = await _authService.ChangePasswordAsync(userId, request);

                _logger.LogInformation("Пароль успешно изменен. UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Ошибка при смене пароля. UserId: {UserId}, Ошибка: {ErrorMessage}",
                    userId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при смене пароля. UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount(DeleteAccountRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Попытка удаления аккаунта без аутентификации");
                return Unauthorized();
            }

            _logger.LogWarning("Удаление аккаунта пользователем. UserId: {UserId}", userId);

            try
            {
                var result = await _authService.DeleteAccountAsync(userId, request);
                Response.Cookies.Delete("refreshToken");

                _logger.LogWarning("Аккаунт успешно удален. UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning("Ошибка при удалении аккаунта. UserId: {UserId}, Ошибка: {ErrorMessage}",
                    userId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении аккаунта. UserId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}