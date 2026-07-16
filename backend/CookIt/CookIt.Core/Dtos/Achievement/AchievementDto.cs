public class AchievementDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int Level { get; set; }
    public int? NextThreshold { get; set; }
    public string IconUrl { get; set; } = string.Empty;
}