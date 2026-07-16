using CookIt.Core.Dtos.Auth;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using CookIt.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CookIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMinioImageStorage _imageStorage;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            IMinioImageStorage imageStorage,
            IRecipeService recipeService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _userManager = userManager;
            _imageStorage = imageStorage;
            _recipeService = recipeService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            _logger.LogDebug("Администратор запрашивает всех пользователей");

            try
            {
                var users = await _userService.GetAllUsersAsync();

                _logger.LogInformation(
                    "Администратор получил {Count} пользователей",
                    users.Count);

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка пользователей администратором");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("avatar")]
        [Authorize]
        public async Task<IActionResult> GetAvatarUrl()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug("Запрос URL аватарки пользователем {UserId}", userId);

            try
            {
                if (userId == null)
                {
                    _logger.LogWarning("Попытка запроса аватарки без аутентификации");
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при запросе аватарки. UserId: {UserId}", userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                if (string.IsNullOrEmpty(user.AvatarKey))
                {
                    _logger.LogDebug("У пользователя {UserId} нет аватарки", userId);
                    return Ok(new
                    {
                        hasAvatar = false,
                        message = "Аватарка не установлена"
                    });
                }

                var avatarUrl = await _imageStorage.GetPreviewUrlAsync(user.AvatarKey);
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    _logger.LogWarning(
                        "Аватарка не найдена в хранилище. UserId: {UserId}, AvatarKey: {AvatarKey}",
                        userId, user.AvatarKey);
                    return Ok(new
                    {
                        hasAvatar = false,
                        message = "Аватарка не найдена в хранилище"
                    });
                }

                _logger.LogDebug(
                    "Аватарка пользователя {UserId} найдена. URL: {AvatarUrl}",
                    userId, avatarUrl);

                return Ok(new
                {
                    hasAvatar = true,
                    avatarUrl = avatarUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении аватарки пользователя {UserId}", userId);
                return BadRequest(new { message = $"Ошибка получения аватарки: {ex.Message}" });
            }
        }

        [HttpPost("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation(
                "Загрузка аватарки пользователем {UserId}. Размер файла: {FileSize}, Тип: {ContentType}",
                userId, file?.Length, file?.ContentType);

            try
            {
                if (userId == null)
                {
                    _logger.LogWarning("Попытка загрузки аватарки без аутентификации");
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при загрузке аватарки. UserId: {UserId}", userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Пустой файл при загрузке аватарки пользователем {UserId}", userId);
                    return BadRequest(new { message = "Файл не выбран" });
                }

                if (!string.IsNullOrEmpty(user.AvatarKey))
                {
                    _logger.LogDebug(
                        "Удаление старой аватарки пользователя {UserId}. Старый ключ: {OldAvatarKey}",
                        userId, user.AvatarKey);
                    await _imageStorage.DeleteImageAsync(user.AvatarKey);
                }

                var avatarKey = await _imageStorage.SaveImageAsync(file);
                user.AvatarKey = avatarKey;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Ошибка при обновлении аватарки пользователя {UserId}. Ошибки: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Ошибка при обновлении аватарки" });
                }

                var avatarUrl = await _imageStorage.GetPreviewUrlAsync(avatarKey);

                _logger.LogInformation(
                    "Аватарка пользователя {UserId} успешно обновлена. Новый ключ: {AvatarKey}",
                    userId, avatarKey);

                return Ok(new
                {
                    message = "Аватарка успешно обновлена",
                    avatarKey = avatarKey,
                    avatarUrl = avatarUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке аватарки пользователя {UserId}", userId);
                return BadRequest(new { message = $"Ошибка загрузки аватарки: {ex.Message}" });
            }
        }

        [HttpDelete("avatar")]
        [Authorize]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Удаление аватарки пользователем {UserId}", userId);

            try
            {
                if (userId == null)
                {
                    _logger.LogWarning("Попытка удаления аватарки без аутентификации");
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при удалении аватарки. UserId: {UserId}", userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                if (string.IsNullOrEmpty(user.AvatarKey))
                {
                    _logger.LogWarning("У пользователя {UserId} нет аватарки для удаления", userId);
                    return BadRequest(new { message = "Аватарка не установлена" });
                }

                await _imageStorage.DeleteImageAsync(user.AvatarKey);
                user.AvatarKey = null;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError(
                        "Ошибка при удалении аватарки пользователя {UserId}. Ошибки: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Ошибка при удалении аватарки" });
                }

                _logger.LogInformation("Аватарка пользователя {UserId} успешно удалена", userId);

                return Ok(new { message = "Аватарка успешно удалена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении аватарки пользователя {UserId}", userId);
                return BadRequest(new { message = $"Ошибка удаления аватарки: {ex.Message}" });
            }
        }

        [HttpGet("{userId}/avatar")]
        public async Task<IActionResult> GetUserAvatarUrl(string userId)
        {
            _logger.LogDebug("Запрос URL аватарки пользователя {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при запросе аватарки. UserId: {UserId}", userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                if (string.IsNullOrEmpty(user.AvatarKey))
                {
                    _logger.LogDebug("У пользователя {UserId} нет аватарки", userId);
                    return Ok(new
                    {
                        hasAvatar = false,
                        message = "Аватарка не установлена"
                    });
                }

                var avatarUrl = await _imageStorage.GetPreviewUrlAsync(user.AvatarKey);
                if (string.IsNullOrEmpty(avatarUrl))
                {
                    _logger.LogWarning(
                        "Аватарка не найдена в хранилище. UserId: {UserId}, AvatarKey: {AvatarKey}",
                        userId, user.AvatarKey);
                    return Ok(new
                    {
                        hasAvatar = false,
                        message = "Аватарка не найдена в хранилище"
                    });
                }

                _logger.LogDebug(
                    "Аватарка пользователя {UserId} найдена. URL: {AvatarUrl}",
                    userId, avatarUrl);

                return Ok(new
                {
                    hasAvatar = true,
                    avatarUrl = avatarUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении аватарки пользователя {UserId}", userId);
                return BadRequest(new { message = $"Ошибка получения аватарки: {ex.Message}" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogDebug("Запрос профиля текущего пользователя. UserId: {UserId}", userId);

            try
            {
                if (userId == null)
                {
                    _logger.LogWarning("Попытка запроса профиля без аутентификации");
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при запросе профиля. UserId: {UserId}", userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var summary = await _recipeService.GetUserRecipesSummaryAsync(userId);
                var recipes = await _recipeService.GetRecipesByUserIdAsync(userId);

                string? avatarUrl = null;
                if (!string.IsNullOrEmpty(user.AvatarKey))
                {
                    avatarUrl = await _imageStorage.GetPreviewUrlAsync(user.AvatarKey);
                }

                _logger.LogInformation(
                    "Профиль пользователя {UserId} успешно получен. Рецептов: {RecipeCount}",
                    userId, summary.RecipesCount);

                return Ok(new
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    AvatarUrl = avatarUrl,
                    Summary = summary,
                    Recipes = recipes,
                    IsBlocked = user.IsBlocked,
                    BlockedReason = user.BlockedReason,
                    BlockedUntil = user.BlockedUntil
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении профиля пользователя {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{userId}/recipes")]
        [Authorize]
        public async Task<IActionResult> GetUserRecipes(string userId)
        {
            _logger.LogDebug("Запрос рецептов пользователя {UserId}", userId);

            try
            {
                var recipes = await _recipeService.GetRecipesByUserIdAsync(userId);

                _logger.LogInformation(
                    "Успешно получено {Count} рецептов пользователя {UserId}",
                    recipes.Count(), userId);

                return Ok(recipes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рецептов пользователя {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{userId}/recipes/summary")]
        [Authorize]
        public async Task<IActionResult> GetUserRecipesSummary(string userId)
        {
            _logger.LogDebug("Запрос сводки рецептов пользователя {UserId}", userId);

            try
            {
                var summary = await _recipeService.GetUserRecipesSummaryAsync(userId);

                _logger.LogInformation(
                    "Сводка рецептов пользователя {UserId}: Рецептов: {RecipesCount}, Средняя оценка: {AverageRating}",
                    userId, summary.RecipesCount, summary.AverageRating);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сводки рецептов пользователя {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{userId}/public-profile")]
        [Authorize]
        public async Task<IActionResult> GetUserPublicProfile(string userId)
        {
            _logger.LogDebug("Запрос публичного профиля пользователя {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь не найден при запросе публичного профиля. UserId: {UserId}", userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var summary = await _recipeService.GetUserRecipesSummaryAsync(userId);
                var recipes = await _recipeService.GetRecipesByUserIdAsync(userId);

                string? avatarUrl = null;
                if (!string.IsNullOrEmpty(user.AvatarKey))
                {
                    avatarUrl = await _imageStorage.GetPreviewUrlAsync(user.AvatarKey);
                }

                _logger.LogInformation(
                    "Публичный профиль пользователя {UserId} успешно получен. Рецептов: {RecipeCount}",
                    userId, summary.RecipesCount);

                return Ok(new
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    FullName = user.FullName,
                    AvatarUrl = avatarUrl,
                    Summary = summary,
                    Recipes = recipes,
                    IsBlocked = user.IsBlocked,
                    BlockedReason = user.BlockedReason,
                    BlockedUntil = user.BlockedUntil
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении публичного профиля пользователя {UserId}", userId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsersForAdmin()
        {
            _logger.LogDebug("Администратор запрашивает подробный список пользователей");

            try
            {
                var users = await _userService.GetAllUsersWithDetailsAsync();

                _logger.LogInformation(
                    "Администратор получил подробный список {Count} пользователей",
                    users.Count);

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении подробного списка пользователей администратором");
                return BadRequest(new { message = $"Ошибка получения пользователей: {ex.Message}" });
            }
        }

        [HttpPost("admin/block")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequestDto request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogWarning(
                "Блокировка пользователя {TargetUserId} администратором {AdminUserId}. Причина: {Reason}, До: {BlockUntil}",
                request.UserId, currentUserId, request.Reason, request.BlockUntil);

            try
            {
                if (currentUserId == request.UserId)
                {
                    _logger.LogWarning("Попытка администратора {AdminUserId} заблокировать самого себя", currentUserId);
                    return BadRequest(new { message = "Нельзя заблокировать самого себя" });
                }

                var user = await _userService.BlockUserAsync(request.UserId, request.Reason, request.BlockUntil);

                _logger.LogWarning(
                    "Пользователь {TargetUserId} успешно заблокирован администратором {AdminUserId}",
                    request.UserId, currentUserId);

                return Ok(new
                {
                    message = "Пользователь успешно заблокирован",
                    user = user
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка блокировки пользователя {TargetUserId}. Ошибка: {ErrorMessage}",
                    request.UserId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка блокировки пользователя {TargetUserId} администратором {AdminUserId}",
                    request.UserId, currentUserId);
                return BadRequest(new { message = $"Ошибка блокировки пользователя: {ex.Message}" });
            }
        }

        [HttpPost("admin/unblock/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnblockUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogWarning(
                "Разблокировка пользователя {TargetUserId} администратором {AdminUserId}",
                userId, currentUserId);

            try
            {
                var user = await _userService.UnblockUserAsync(userId);

                _logger.LogWarning(
                    "Пользователь {TargetUserId} успешно разблокирован администратором {AdminUserId}",
                    userId, currentUserId);

                return Ok(new
                {
                    message = "Пользователь успешно разблокирован",
                    user = user
                });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(
                    "Ошибка разблокировки пользователя {TargetUserId}. Ошибка: {ErrorMessage}",
                    userId, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка разблокировки пользователя {TargetUserId} администратором {AdminUserId}",
                    userId, currentUserId);
                return BadRequest(new { message = $"Ошибка разблокировки пользователя: {ex.Message}" });
            }
        }

        [HttpGet("admin/{userId}/details")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserDetailsForAdmin(string userId)
        {
            _logger.LogDebug(
                "Администратор запрашивает детальную информацию о пользователе {UserId}",
                userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning(
                        "Пользователь не найден при запросе детальной информации администратором. UserId: {UserId}",
                        userId);
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var summary = await _recipeService.GetUserRecipesSummaryAsync(userId);
                var recipes = await _recipeService.GetRecipesByUserIdAsync(userId);

                string? avatarUrl = null;
                if (!string.IsNullOrEmpty(user.AvatarKey))
                {
                    avatarUrl = await _imageStorage.GetPreviewUrlAsync(user.AvatarKey);
                }

                _logger.LogInformation(
                    "Детальная информация о пользователе {UserId} успешно получена администратором",
                    userId);

                return Ok(new
                {
                    UserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount,
                    IsBlocked = user.IsBlocked,
                    BlockedReason = user.BlockedReason,
                    BlockedUntil = user.BlockedUntil,
                    AvatarUrl = avatarUrl,
                    Roles = roles,
                    Summary = summary
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при получении детальной информации о пользователе {UserId} администратором",
                    userId);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}