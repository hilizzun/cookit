using System.Text.Json.Serialization;

namespace CookIt.Core.Dtos.AI
{
    public class CommentModerationResult
    {
        [JsonPropertyName("isOffensive")]
        public bool IsOffensive { get; set; }

        [JsonPropertyName("isOffTopic")]
        public bool IsOffTopic { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}