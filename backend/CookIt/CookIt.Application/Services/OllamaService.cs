using CookIt.Core.Dtos.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CookIt.Application.Services;

public class OllamaService : IAiService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(IChatClient chatClient, ILogger<OllamaService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<List<string>> GetFactsForRecipeAsync(string recipeName)
    {
        var prompt = $"Ты — кулинарный эксперт. Напиши 3 коротких интересных факта о блюде «{recipeName}». Требования: каждый факт не длиннее 150 символов, не используй нумерацию и маркеры, каждый факт с новой строки, пиши только на русском языке. Перепроверяй несколько раз на наличие нерусских слов, лексические, грамматические, логические ошибки. Пример: Кофе был открыт эфиопскими пастухами, заметившими бодрость коз. В Турции в старину муж мог развестись с женой, если та не могла сварить хороший кофе. Зелёный и чёрный чай — это листья одного растения, но разной степени ферментации.";
        return await GetFactsFromAiAsync(prompt);
    }

    public async Task<List<string>> GetFactsForIngredientAsync(string ingredientName)
    {
        var prompt = $"Ты — кулинарный эксперт. Напиши 3 коротких интересных факта об ингредиенте «{ingredientName}». Требования: каждый факт не длиннее 150 символов, не используй нумерацию и маркеры, каждый факт с новой строки, пиши только на русском языке. Перепроверяй несколько раз на наличие нерусских слов, лексические, грамматические, логические ошибки. Пример: Кофе был открыт эфиопскими пастухами, заметившими бодрость коз. В Турции в старину муж мог развестись с женой, если та не могла сварить хороший кофе. Зелёный и чёрный чай — это листья одного растения, но разной степени ферментации.";
        return await GetFactsFromAiAsync(prompt);
    }

    private async Task<List<string>> GetFactsFromAiAsync(string prompt)
    {
        try
        {
            var response = await _chatClient.GetResponseAsync(prompt);
            var assistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);
            if (assistantMessage == null)
            {
                _logger.LogWarning("Не удалось получить ответ от модели");
                return new List<string>();
            }
            var text = assistantMessage.Text;
            var facts = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                               .Select(f => f.Trim())
                               .Where(f => !string.IsNullOrWhiteSpace(f))
                               .ToList() ?? new List<string>();
            return facts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации фактов через Ollama");
            return new List<string> { "Не удалось загрузить интересные факты." };
        }
    }

    public async Task<CommentModerationResult> ModerateCommentAsync(string commentContent)
    {
        var prompt = $@"Ты — автоматический модератор кулинарного сайта. Твоя задача — проверить комментарий на наличие нарушений.

Правила:
1. Нецензурная лексика (мат, оскорбления, грубость) — считается нарушением (isOffensive = true).
2. Оффтоп (комментарий не по теме кулинарии, рецепта, приготовления, ингредиентов, посуды, кухонных техник и т.п.) — считается нарушением (isOffTopic = true).
3. Обсуждение политики, погоды, спорта и т.п. — это явный оффтоп.
4. Если комментарий имеет отношение к еде, готовке, ресторанам, продуктам — это НЕ оффтоп.

Комментарий пользователя: ""{commentContent}""

Ответь строго в формате JSON без дополнительных пояснений:
{{
    ""isOffensive"": true/false,
    ""isOffTopic"": true/false,
    ""reason"": ""краткая причина (например, 'нецензурная лексика' или 'оффтоп: шины')""
}}

Примеры:
Комментарий: ""Приготовил этот пирог, очень вкусно!"" → {{ ""isOffensive"": false, ""isOffTopic"": false, ""reason"": """" }}
Комментарий: ""Иди ты ..."" → {{ ""isOffensive"": true, ""isOffTopic"": false, ""reason"": ""нецензурная лексика"" }}
Комментарий: ""Какие шины взять на лето? В магазине на Кутузова хорошие."" → {{ ""isOffensive"": false, ""isOffTopic"": true, ""reason"": ""оффтоп (автомобили)"" }}
";

        var response = await _chatClient.GetResponseAsync(prompt);
        var assistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);
        if (assistantMessage == null)
        {
            _logger.LogWarning("Не удалось получить ответ от модели при модерации комментария");
            return new CommentModerationResult { IsOffensive = false, IsOffTopic = false };
        }
        var text = assistantMessage.Text;
        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}') + 1;
        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var json = text.Substring(jsonStart, jsonEnd - jsonStart);
            try
            {
                _logger.LogInformation("Raw response from AI: {Text}", text);
                var result = JsonSerializer.Deserialize<CommentModerationResult>(json);
                return result ?? new CommentModerationResult { IsOffensive = false, IsOffTopic = false };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка парсинга JSON при модерации комментария. Текст: {Text}", text);
                return new CommentModerationResult { IsOffensive = false, IsOffTopic = false };
            }
        }
        return new CommentModerationResult { IsOffensive = false, IsOffTopic = false };
    }
    public async Task<IngredientNutritionDto> GetIngredientNutritionAsync(string ingredientName)
    {
        _logger.LogInformation("Запрос КБЖУ для ингредиента: {IngredientName}", ingredientName);

        var prompt = $@"Ты — справочная система по пищевой ценности продуктов. 
На запрос пользователя ты должен вернуть ТОЛЬКО JSON-объект с полями calories, proteins, fats, carbohydrates для ингредиента «{ingredientName}» на 100 грамм. 
Не добавляй никаких пояснений, не пиши лишний текст. 
Пример ответа: {{""calories"": 20.0, ""proteins"": 1.0, ""fats"": 0.2, ""carbohydrates"": 4.0}}";

        try
        {
            var response = await _chatClient.GetResponseAsync(prompt);
            var assistantMessage = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);

            if (assistantMessage == null)
            {
                _logger.LogWarning("Ответ от AI не содержит сообщения ассистента для ингредиента: {IngredientName}", ingredientName);
                return new IngredientNutritionDto();
            }

            var text = assistantMessage.Text;
            _logger.LogDebug("Raw AI response for {IngredientName}: {RawText}", ingredientName, text);

            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = text.Substring(jsonStart, jsonEnd - jsonStart);
                _logger.LogDebug("Extracted JSON for {IngredientName}: {Json}", ingredientName, json);

                try
                {
                    var result = JsonSerializer.Deserialize<IngredientNutritionDto>(json);
                    if (result == null)
                    {
                        _logger.LogWarning("Deserialization returned null for {IngredientName}. JSON: {Json}", ingredientName, json);
                        return new IngredientNutritionDto();
                    }

                    _logger.LogInformation(
                        "Successfully parsed KBJU for {IngredientName}: Calories={Calories}, Proteins={Proteins}, Fats={Fats}, Carbs={Carbohydrates}",
                        ingredientName, result.Calories, result.Proteins, result.Fats, result.Carbohydrates);

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON parsing error for {IngredientName}. Raw JSON: {Json}", ingredientName, json);
                    return new IngredientNutritionDto();
                }
            }
            else
            {
                _logger.LogWarning("No JSON object found in AI response for {IngredientName}. Raw text: {RawText}", ingredientName, text);
                return new IngredientNutritionDto();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while getting KBJU for {IngredientName}", ingredientName);
            return new IngredientNutritionDto();
        }
    }
}