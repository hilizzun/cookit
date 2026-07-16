using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CookIt.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IRecipeRepository _recipeRepository;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(
            IFavoriteRepository favoriteRepository,
            IRecipeRepository recipeRepository,
            IUserStatisticsService userStatisticsService,
            ILogger<FavoriteService> logger)
        {
            _favoriteRepository = favoriteRepository;
            _recipeRepository = recipeRepository;
            _userStatisticsService = userStatisticsService;
            _logger = logger;
        }

        public async Task AddToFavoritesAsync(string userId, int recipeId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe == null)
                throw new Exception("Рецепт не найден");

            var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, recipeId);
            if (isFavorite)
                throw new Exception("Рецепт уже в избранном");

            await _favoriteRepository.AddFavoriteAsync(userId, recipeId);

            if (recipe.CreatorId != userId)
            {
                await _userStatisticsService.IncrementFavoritesAddedAsync(userId);
                _logger.LogDebug("Пользователь {UserId} добавил в избранное рецепт автора {AuthorId}, статистика обновлена", userId, recipe.CreatorId);
            }
        }

        public async Task RemoveFromFavoritesAsync(string userId, int recipeId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe == null)
                throw new Exception("Рецепт не найден");

            var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, recipeId);
            if (!isFavorite)
                throw new Exception("Рецепт не в избранном");

            await _favoriteRepository.RemoveFavoriteAsync(userId, recipeId);

            if (recipe.CreatorId != userId)
            {
                await _userStatisticsService.DecrementFavoritesAddedAsync(userId);
                _logger.LogDebug("Пользователь {UserId} удалил из избранного рецепт автора {AuthorId}, статистика обновлена", userId, recipe.CreatorId);
            }
        }

        public async Task<bool> IsFavoriteAsync(string userId, int recipeId)
        {
            return await _favoriteRepository.IsFavoriteAsync(userId, recipeId);
        }

        public async Task<IEnumerable<RecipeDto>> GetUserFavoritesAsync(string userId)
        {
            var favoriteIds = await _favoriteRepository.GetFavoriteRecipeIdsAsync(userId);
            if (!favoriteIds.Any())
                return new List<RecipeDto>();

            var recipes = await _recipeRepository.GetByIdsAsync(favoriteIds);
            return recipes;
        }
    }
}