using System.Text.Json.Serialization;

namespace CookIt.Core.Dtos.AI
{

    public class IngredientNutritionDto
    {
        [JsonPropertyName("calories")]
        public double Calories { get; set; }

        [JsonPropertyName("proteins")]
        public double Proteins { get; set; }

        [JsonPropertyName("fats")]
        public double Fats { get; set; }

        [JsonPropertyName("carbohydrates")]
        public double Carbohydrates { get; set; }
    }
}