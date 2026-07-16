using CookIt.Core.Dtos.Achievement;
using CookIt.Core.Entities;
using CookIt.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CookIt.Application.Services
{
    public class UserStatisticsService : IUserStatisticsService
    {
        private readonly CookItContext _context;
        private readonly ILogger<UserStatisticsService> _logger;

        public UserStatisticsService(CookItContext context, ILogger<UserStatisticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<UserStatistics> GetOrCreateStatisticsAsync(string userId)
        {
            var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId);
            if (stats == null)
            {
                stats = new UserStatistics { UserId = userId, UpdatedAt = DateTime.UtcNow };
                _context.UserStatistics.Add(stats);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Создана новая статистика для пользователя {UserId}", userId);
            }
            return stats;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            return new UserStatisticsDto
            {
                RecipesPublished = stats.RecipesPublished,
                FavoritesAdded = stats.FavoritesAdded,
                ShoppingListAdded = stats.ShoppingListAdded,
                CommentsLeft = stats.CommentsLeft,
                WheelSpins = stats.WheelSpins,
                FiveStarRatingsReceived = stats.FiveStarRatingsReceived
            };
        }

        public async Task IncrementRecipesPublishedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            stats.RecipesPublished++;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogDebug("Увеличено RecipesPublished для {UserId}: {Value}", userId, stats.RecipesPublished);
        }

        public async Task IncrementFavoritesAddedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            stats.FavoritesAdded++;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DecrementFavoritesAddedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            if (stats.FavoritesAdded > 0)
            {
                stats.FavoritesAdded--;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Попытка уменьшить FavoritesAdded для {UserId}, но значение уже 0", userId);
            }
        }

        public async Task IncrementShoppingListAddedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            stats.ShoppingListAdded++;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DecrementShoppingListAddedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            if (stats.ShoppingListAdded > 0)
            {
                stats.ShoppingListAdded--;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Попытка уменьшить ShoppingListAdded для {UserId}, но значение уже 0", userId);
            }
        }

        public async Task IncrementCommentsLeftAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            stats.CommentsLeft++;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DecrementCommentsLeftAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            if (stats.CommentsLeft > 0)
            {
                stats.CommentsLeft--;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Попытка уменьшить CommentsLeft для {UserId}, но значение уже 0", userId);
            }
        }

        public async Task IncrementWheelSpinsAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            stats.WheelSpins++;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task IncrementFiveStarRatingsReceivedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            stats.FiveStarRatingsReceived++;
            stats.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DecrementFiveStarRatingsReceivedAsync(string userId)
        {
            var stats = await GetOrCreateStatisticsAsync(userId);
            if (stats.FiveStarRatingsReceived > 0)
            {
                stats.FiveStarRatingsReceived--;
                stats.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Попытка уменьшить FiveStarRatingsReceived для {UserId}, но значение уже 0", userId);
            }
        }
    }
}