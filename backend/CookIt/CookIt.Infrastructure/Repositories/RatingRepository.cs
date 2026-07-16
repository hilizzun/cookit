using CookIt.Core.Dtos.Ratings;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly CookItContext _context;

        public RatingRepository(CookItContext context)
        {
            _context = context;
        }

        public async Task<RecipeRating?> GetUserRatingAsync(string userId, int recipeId)
        {
            return await _context.RecipeRatings
                .FirstOrDefaultAsync(rr => rr.UserId == userId && rr.RecipeId == recipeId);
        }

        public async Task AddOrUpdateRatingAsync(string userId, int recipeId, int value)
        {
            var existingRating = await GetUserRatingAsync(userId, recipeId);

            if (existingRating != null)
            {
                existingRating.Value = value;
                existingRating.RatedAt = DateTime.UtcNow;
                _context.RecipeRatings.Update(existingRating);
            }
            else
            {
                var newRating = new RecipeRating
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    Value = value,
                    RatedAt = DateTime.UtcNow
                };
                await _context.RecipeRatings.AddAsync(newRating);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<RecipeRatingSummaryDto> GetRatingSummaryAsync(int recipeId, string? userId = null)
        {
            var ratings = _context.RecipeRatings
                .Where(rr => rr.RecipeId == recipeId);

            var averageRating = await ratings.AverageAsync(rr => (double?)rr.Value) ?? 0;
            var totalRatings = await ratings.CountAsync();

            var distribution = await ratings
                .GroupBy(rr => rr.Value)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            var fullDistribution = Enumerable.Range(1, 5)
                .ToDictionary(r => r, r => distribution.ContainsKey(r) ? distribution[r] : 0);

            int? userRating = null;
            if (!string.IsNullOrEmpty(userId))
            {
                var userRatingEntity = await GetUserRatingAsync(userId, recipeId);
                userRating = userRatingEntity?.Value;
            }

            return new RecipeRatingSummaryDto
            {
                AverageRating = Math.Round(averageRating, 1),
                TotalRatings = totalRatings,
                UserRating = userRating,
                RatingDistribution = fullDistribution
            };
        }

        public async Task<double> GetAverageRatingAsync(int recipeId)
        {
            var average = await _context.RecipeRatings
                .Where(rr => rr.RecipeId == recipeId)
                .AverageAsync(rr => (double?)rr.Value);

            return average ?? 0;
        }

        public async Task<int> GetTotalRatingsCountAsync(int recipeId)
        {
            return await _context.RecipeRatings
                .CountAsync(rr => rr.RecipeId == recipeId);
        }

        public async Task<bool> DeleteRatingAsync(string userId, int recipeId)
        {
            var rating = await GetUserRatingAsync(userId, recipeId);
            if (rating == null) return false;

            _context.RecipeRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserRatedRecipeAsync(string userId, int recipeId)
        {
            return await _context.RecipeRatings
                .AnyAsync(rr => rr.UserId == userId && rr.RecipeId == recipeId);
        }
    }
}