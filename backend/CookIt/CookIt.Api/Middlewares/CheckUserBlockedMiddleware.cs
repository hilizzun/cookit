using CookIt.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CookIt.Api.Middleware
{
    public class CheckUserBlockedMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckUserBlockedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            // Проверяем, аутентифицирован ли пользователь
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await userManager.FindByIdAsync(userId);

                    // Проверяем, заблокирован ли пользователь
                    if (user != null && user.IsBlocked)
                    {
                        // Исключаем GET запросы и админские маршруты
                        if (!context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                            !context.Request.Path.StartsWithSegments("/api/users/admin") &&
                            !context.Request.Path.StartsWithSegments("/api/auth/logout") &&
                            !context.Request.Path.StartsWithSegments("/api/auth/refresh"))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                message = "Ваш аккаунт заблокирован",
                                reason = user.BlockedReason,
                                blockedUntil = user.BlockedUntil
                            });
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}