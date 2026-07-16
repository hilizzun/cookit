using CookIt.Core.Dtos.Achievement;

public interface IUserStatisticsService
{
    Task<UserStatisticsDto> GetUserStatisticsAsync(string userId);
    Task IncrementRecipesPublishedAsync(string userId);
    Task IncrementFavoritesAddedAsync(string userId);
    Task DecrementFavoritesAddedAsync(string userId);
    Task IncrementShoppingListAddedAsync(string userId);
    Task DecrementShoppingListAddedAsync(string userId);
    Task IncrementCommentsLeftAsync(string userId);
    Task DecrementCommentsLeftAsync(string userId);
    Task IncrementWheelSpinsAsync(string userId);
    Task IncrementFiveStarRatingsReceivedAsync(string userId);
    Task DecrementFiveStarRatingsReceivedAsync(string userId);
}