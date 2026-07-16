using CookIt.Core.Dtos.Achievement;

namespace CookIt.Core.Interfaces
{
    public interface IAchievementService
    {
        List<AchievementDto> CalculateAchievements(UserStatisticsDto stats);
    }
}