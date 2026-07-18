using CookIt.Core.Dtos.Achievement;
using CookIt.Core.Interfaces;
using CookIt.Core.Settings;
using Microsoft.Extensions.Configuration;

namespace CookIt.Application.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly AchievementSettings _settings;
        private readonly string _baseUrl;

        public AchievementService(AchievementSettings settings, IConfiguration configuration)
        {
            _settings = settings;
            _baseUrl = configuration["BaseUrl"] ?? "https://localhost:7031";
        }

        public List<AchievementDto> CalculateAchievements(UserStatisticsDto stats)
        {
            var result = new List<AchievementDto>();
            var mapping = new Dictionary<string, Func<UserStatisticsDto, int>>
            {
                ["RecipesPublished"] = s => s.RecipesPublished,
                ["FavoritesAdded"] = s => s.FavoritesAdded,
                ["ShoppingListAdded"] = s => s.ShoppingListAdded,
                ["CommentsLeft"] = s => s.CommentsLeft,
                ["WheelSpins"] = s => s.WheelSpins,
                ["FiveStarRatingsReceived"] = s => s.FiveStarRatingsReceived
            };

            foreach (var kvp in mapping)
            {
                var type = kvp.Key;
                var currentValue = kvp.Value(stats);
                if (!_settings.Achievements.TryGetValue(type, out var def))
                    continue;

                int level = 0;
                int? nextThreshold = null;
                foreach (var threshold in def.Thresholds.OrderBy(t => t))
                {
                    if (currentValue >= threshold)
                        level++;
                    else
                    {
                        nextThreshold = threshold;
                        break;
                    }
                }

                string iconUrl;
                if (level == 0)
                {
                    var firstThreshold = def.Thresholds[0];
                    iconUrl = def.Icons.GetValueOrDefault($"locked_{firstThreshold}", def.Icons["locked_1"]);
                }
                else if (level == def.Thresholds.Count)
                {
                    var maxThreshold = def.Thresholds.Last();
                    iconUrl = def.Icons.GetValueOrDefault(maxThreshold.ToString(), def.Icons[maxThreshold.ToString()]);
                }
                else
                {
                    var next = nextThreshold.Value;
                    iconUrl = def.Icons.GetValueOrDefault($"locked_{next}", def.Icons[$"locked_{def.Thresholds[0]}"]);
                }

                string fullIconUrl = $"{_baseUrl}{iconUrl}";

                result.Add(new AchievementDto
                {
                    Type = type,
                    Title = def.Title,
                    CurrentValue = currentValue,
                    Level = level,
                    NextThreshold = nextThreshold,
                    IconUrl = fullIconUrl
                });
            }
            return result;
        }
    }
}