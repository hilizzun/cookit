using CookIt.Core.Dtos.Auth;
using System.Text;
using System.Text.Json;

public class UseRefreshTokenFromCookieMiddleware
{
    private readonly RequestDelegate _next;

    public UseRefreshTokenFromCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.Equals("/api/auth/refresh", StringComparison.OrdinalIgnoreCase) &&
            context.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
        {
            context.Request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(
                context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; 
            }

            RefreshTokenRequestDto bodyJson = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(body))
                {
                    bodyJson = JsonSerializer.Deserialize<RefreshTokenRequestDto>(body);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка десериализации: {ex.Message}");
            }

            if (bodyJson == null)
            {
                bodyJson = new RefreshTokenRequestDto
                {
                    RefreshToken = refreshToken
                };
            }

            var updatedBody = JsonSerializer.Serialize(bodyJson);
            var updatedBodyBytes = Encoding.UTF8.GetBytes(updatedBody);

            context.Request.Body = new MemoryStream(updatedBodyBytes);
            context.Request.Body.Position = 0; 
        }

        await _next(context);
    }
}
