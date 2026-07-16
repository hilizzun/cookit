namespace CookIt.Core.Dtos.Ratings
{
    public class RecipeRatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int? UserRating { get; set; } 
        public Dictionary<int, int> RatingDistribution { get; set; } = new(); 
    }
}