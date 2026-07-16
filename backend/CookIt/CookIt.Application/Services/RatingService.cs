using CookIt.Core.Dtos.Ratings;
using CookIt.Core.Interfaces;

namespace CookIt.Application.Services
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IUserStatisticsService _userStatisticsService;

        public RatingService(IRatingRepository ratingRepository, IRecipeRepository recipeRepository,
            IUserStatisticsService userStatisticsService)
        {
            _ratingRepository = ratingRepository;
            _recipeRepository = recipeRepository;
            _userStatisticsService = userStatisticsService;
        }

        public async Task RateRecipeAsync(string userId, int recipeId, int value)
        {
            if (value < 1 || value > 5)
            {
                throw new ArgumentException("Rating value must be between 1 and 5");
            }

            bool isCreator = await _recipeRepository.IsCreatorAsync(recipeId, userId);
            if (isCreator)
            {
                throw new InvalidOperationException("You cannot rate your own recipe");
            }

            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe == null)
            {
                throw new ArgumentException("Recipe not found");
            }

            await _ratingRepository.AddOrUpdateRatingAsync(userId, recipeId, value);
            if (value == 5 && recipe.CreatorId != userId)
            {
                await _userStatisticsService.IncrementFiveStarRatingsReceivedAsync(recipe.CreatorId);
            }
        }

        public async Task<RecipeRatingSummaryDto> GetRatingSummaryAsync(int recipeId, string? userId = null)
        {
            return await _ratingRepository.GetRatingSummaryAsync(recipeId, userId);
        }

        public async Task<int?> GetUserRatingAsync(string userId, int recipeId)
        {
            var rating = await _ratingRepository.GetUserRatingAsync(userId, recipeId);
            return rating?.Value;
        }

        public async Task<bool> RemoveRatingAsync(string userId, int recipeId)
        {
            var oldRating = await _ratingRepository.GetUserRatingAsync(userId, recipeId);
            var result = await _ratingRepository.DeleteRatingAsync(userId, recipeId);
            if (result && oldRating?.Value == 5)
            {
                var recipe = await _recipeRepository.GetByIdAsync(recipeId);
                if (recipe != null && recipe.CreatorId != userId)
                    await _userStatisticsService.DecrementFiveStarRatingsReceivedAsync(recipe.CreatorId);
            }
            return result;
        }
    }
}