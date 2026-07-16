namespace CookIt.Core.Settings
{
    public class AchievementSettings
    {
        public Dictionary<string, AchievementDefinition> Achievements { get; set; } = new();
    }

    public class AchievementDefinition
    {
        public string Title { get; set; } = string.Empty;
        public List<int> Thresholds { get; set; } = new();
        public Dictionary<string, string> Icons { get; set; } = new();
    }
}