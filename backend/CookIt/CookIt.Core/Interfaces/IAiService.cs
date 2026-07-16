using CookIt.Core.Dtos.AI;

public interface IAiService
{
    Task<List<string>> GetFactsForRecipeAsync(string recipeName);
    Task<List<string>> GetFactsForIngredientAsync(string ingredientName);
    Task<CommentModerationResult> ModerateCommentAsync(string commentContent);
    Task<IngredientNutritionDto> GetIngredientNutritionAsync(string ingredientName);
}