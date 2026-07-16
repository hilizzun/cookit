using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Dtos.Users;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CookIt.Application.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _repository;
        private readonly IIngredientRepository _ingredientRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IMinioImageStorage _imageStorage;
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IRatingService _ratingService;
        private readonly IUserStatisticsService _userStatisticsService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(
            IRecipeRepository repository,
            IIngredientRepository ingredientRepository,
            IUnitRepository unitRepository,
            IMinioImageStorage imageStorage,
            IFavoriteRepository favoriteRepository,
            IUserStatisticsService userStatisticsService,
            IRatingService ratingService,
            UserManager<ApplicationUser> userManager,
            ILogger<RecipeService> logger)
        {
            _repository = repository;
            _ingredientRepository = ingredientRepository;
            _unitRepository = unitRepository;
            _imageStorage = imageStorage;
            _favoriteRepository = favoriteRepository;
            _userStatisticsService = userStatisticsService;
            _ratingService = ratingService;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task CalculateNutritionInfoAsync(RecipeDto recipe)
        {
            using (_logger.BeginScope(new { RecipeId = recipe.Id, RecipeName = recipe.Name }))
            {
                _logger.LogDebug("Начало расчета нутриентов для рецепта");

                double totalWeight = 0;
                double totalCalories = 0;
                double totalProteins = 0;
                double totalFats = 0;
                double totalCarbohydrates = 0;

                if (recipe.RecipeIngredients == null || !recipe.RecipeIngredients.Any())
                {
                    _logger.LogWarning("Ингредиенты не найдены для расчета нутриентов");
                    return;
                }

                _logger.LogDebug("Количество ингредиентов для расчета: {IngredientCount}",
                    recipe.RecipeIngredients.Count());

                foreach (var ingredientDto in recipe.RecipeIngredients)
                {
                    var ingredient = await _ingredientRepository.GetByIdAsync(ingredientDto.IngredientId);
                    if (ingredient == null)
                    {
                        _logger.LogWarning("Ингредиент не найден для расчета нутриентов. IngredientId: {IngredientId}",
                            ingredientDto.IngredientId);
                        continue;
                    }

                    double quantity = ingredientDto.Quantity ?? 0;
                    double weightInGrams = quantity;

                    if (ingredientDto.UnitId.HasValue)
                    {
                        var unit = await _unitRepository.GetByIdAsync(ingredientDto.UnitId.Value);
                        if (unit?.ConversionToGrams.HasValue == true)
                        {
                            weightInGrams = quantity * unit.ConversionToGrams.Value;
                        }
                    }

                    double factor = ingredient.IsByPiece ? quantity : weightInGrams / 100.0;

                    double ingredientCalories = ingredient.Calories * factor;
                    double ingredientProteins = ingredient.Proteins * factor;
                    double ingredientFats = ingredient.Fats * factor;
                    double ingredientCarbs = ingredient.Carbohydrates * factor;

                    totalCalories += ingredientCalories;
                    totalProteins += ingredientProteins;
                    totalFats += ingredientFats;
                    totalCarbohydrates += ingredientCarbs;
                    totalWeight += weightInGrams;
                }

                recipe.TotalCalories = Math.Round(totalCalories, 1);
                recipe.TotalProteins = Math.Round(totalProteins, 1);
                recipe.TotalFats = Math.Round(totalFats, 1);
                recipe.TotalCarbohydrates = Math.Round(totalCarbohydrates, 1);

                if (recipe.Servings.HasValue && recipe.Servings > 0)
                {
                    recipe.CaloriesPerServing = Math.Round(totalCalories / recipe.Servings.Value, 1);
                    recipe.ProteinsPerServing = Math.Round(totalProteins / recipe.Servings.Value, 1);
                    recipe.FatsPerServing = Math.Round(totalFats / recipe.Servings.Value, 1);
                    recipe.CarbohydratesPerServing = Math.Round(totalCarbohydrates / recipe.Servings.Value, 1);
                }
                else
                {
                    recipe.CaloriesPerServing = 0;
                    recipe.ProteinsPerServing = 0;
                    recipe.FatsPerServing = 0;
                    recipe.CarbohydratesPerServing = 0;
                }

                if (totalWeight > 0)
                {
                    recipe.CaloriesPer100g = Math.Round((totalCalories / totalWeight) * 100, 1);
                    recipe.ProteinsPer100g = Math.Round((totalProteins / totalWeight) * 100, 1);
                    recipe.FatsPer100g = Math.Round((totalFats / totalWeight) * 100, 1);
                    recipe.CarbohydratesPer100g = Math.Round((totalCarbohydrates / totalWeight) * 100, 1);
                }
                else
                {
                    recipe.CaloriesPer100g = 0;
                    recipe.ProteinsPer100g = 0;
                    recipe.FatsPer100g = 0;
                    recipe.CarbohydratesPer100g = 0;
                }

                _logger.LogInformation(
                    "Расчет нутриентов завершен. " +
                    "Итог: {TotalCalories} ккал, {TotalProteins} белков, {TotalFats} жиров, {TotalCarbs} углеводов, " +
                    "На порцию: {CaloriesPerServing} ккал, {ProteinsPerServing} белков, {FatsPerServing} жиров, {CarbsPerServing} углеводов, " +
                    "На 100г: {CaloriesPer100g} ккал, {ProteinsPer100g} белков, {FatsPer100g} жиров, {CarbsPer100g} углеводов",
                    recipe.TotalCalories, recipe.TotalProteins, recipe.TotalFats, recipe.TotalCarbohydrates,
                    recipe.CaloriesPerServing, recipe.ProteinsPerServing, recipe.FatsPerServing, recipe.CarbohydratesPerServing,
                    recipe.CaloriesPer100g, recipe.ProteinsPer100g, recipe.FatsPer100g, recipe.CarbohydratesPer100g);
            }
        }

        public async Task<IEnumerable<RecipeWheelItemDto>> GetRandomRecipesForWheelAsync(int count, string? userId = null)
        {
            using (_logger.BeginScope(new { Count = count, UserId = userId }))
            {
                _logger.LogDebug("Получение случайных рецептов для колеса. Count: {Count}", count);

                var allApproved = await _repository.GetAllApprovedAsync();
                if (!allApproved.Any())
                {
                    _logger.LogWarning("Нет утверждённых рецептов для колеса");
                    return Enumerable.Empty<RecipeWheelItemDto>();
                }

                var random = new Random();
                var selected = allApproved.OrderBy(x => random.Next()).Take(count).ToList();

                var result = new List<RecipeWheelItemDto>();
                foreach (var recipe in selected)
                {
                    var dto = new RecipeWheelItemDto
                    {
                        Id = recipe.Id,
                        Name = recipe.Name
                    };
                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            dto.PreviewImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Не удалось получить URL изображения для рецепта {RecipeId}", recipe.Id);
                        }
                    }
                    result.Add(dto);
                }

                _logger.LogInformation("Возвращено {Count} случайных рецептов", result.Count);
                return result;
            }
        }

        public async Task<(IEnumerable<RecipeDto> Recipes, int TotalCount)> GetAllRecipesAsync(
            RecipeFilterDto? filter = null, string? userId = null)
        {
            using (_logger.BeginScope(new { UserId = userId, Filter = filter }))
            {
                _logger.LogDebug("Начало получения всех рецептов");

                var user = await _userManager.FindByIdAsync(userId ?? "");
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
                var isAdminOrModerator = roles.Any(r => r == "Admin" || r == "Moderator");

                _logger.LogDebug("Пользователь {UserId} является администратором/модератором: {IsAdminOrModerator}",
                    userId, isAdminOrModerator);

                var (recipes, totalCount) = await _repository.GetAllAsync(filter, userId, includeUnapproved: isAdminOrModerator);

                _logger.LogInformation("Получено {RecipeCount} рецептов из {TotalCount}",
                    recipes.Count(), totalCount);

                await AddRatingsToRecipesAsync(recipes, userId);

                foreach (var recipe in recipes)
                {
                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            recipe.ImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                            recipe.PreviewImageUrl = recipe.ImageUrl;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Ошибка при получении URL изображения рецепта {RecipeId}. ImagePath: {ImagePath}",
                                recipe.Id, recipe.ImagePath);
                        }
                    }
                }

                await AddCreatorInfoToRecipesAsync(recipes);

                return (recipes, totalCount);
            }
        }

        public async Task<RecipeDto?> GetRecipeByIdAsync(int id, string? userId = null)
        {
            using (_logger.BeginScope(new { RecipeId = id, UserId = userId }))
            {
                _logger.LogDebug("Начало получения рецепта по ID");

                var recipe = await _repository.GetByIdAsync(id);

                if (recipe == null)
                {
                    _logger.LogWarning("Рецепт не найден. RecipeId: {RecipeId}", id);
                    return null;
                }

                var user = await _userManager.FindByIdAsync(userId ?? "");
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
                var isAdminOrModerator = roles.Any(r => r == "Admin" || r == "Moderator");

                if (recipe.IsApproved == true ||
                    recipe.CreatorId == userId ||
                    isAdminOrModerator)
                {
                    _logger.LogDebug("Пользователь имеет доступ к рецепту. UserId: {UserId}, IsAdminOrModerator: {IsAdminOrModerator}, IsCreator: {IsCreator}",
                        userId, isAdminOrModerator, recipe.CreatorId == userId);

                    if (!string.IsNullOrEmpty(userId))
                    {
                        recipe.IsFavorite = await _favoriteRepository.IsFavoriteAsync(userId, id);
                        var ratingSummary = await _ratingService.GetRatingSummaryAsync(id, userId);
                        recipe.AverageRating = ratingSummary.AverageRating;
                        recipe.TotalRatings = ratingSummary.TotalRatings;
                        recipe.UserRating = ratingSummary.UserRating;
                    }

                    var creator = await _userManager.FindByIdAsync(recipe.CreatorId);
                    if (creator != null)
                    {
                        recipe.CreatorUsername = creator.UserName;
                        if (!string.IsNullOrEmpty(creator.AvatarKey))
                        {
                            try
                            {
                                recipe.CreatorAvatarUrl = await _imageStorage.GetPreviewUrlAsync(creator.AvatarKey);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex,
                                    "Ошибка при получении аватара создателя. CreatorId: {CreatorId}, AvatarKey: {AvatarKey}",
                                    creator.Id, creator.AvatarKey);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            recipe.ImageUrl = await _imageStorage.GetOriginalUrlAsync(recipe.ImagePath);
                            recipe.OriginalImageUrl = recipe.ImageUrl;
                            recipe.PreviewImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Ошибка при получении URL изображения рецепта. RecipeId: {RecipeId}, ImagePath: {ImagePath}",
                                id, recipe.ImagePath);
                        }
                    }

                    _logger.LogInformation("Рецепт успешно получен. RecipeId: {RecipeId}", id);
                    return recipe;
                }
                else
                {
                    _logger.LogWarning(
                        "Пользователь не имеет доступа к рецепту. UserId: {UserId}, RecipeId: {RecipeId}, IsApproved: {IsApproved}",
                        userId, id, recipe.IsApproved);
                    return null;
                }
            }
        }

        public async Task<RecipeDto> AddRecipeAsync(RecipeCreateRequest request, string creatorId)
        {
            using (_logger.BeginScope(new { CreatorId = creatorId, RecipeName = request.Name }))
            {
                _logger.LogInformation("Начало создания рецепта. Название: {RecipeName}, Тип блюда: {DishTypeId}",
                    request.Name, request.DishTypeId);

                string? imageKey = null;

                if (request.Image != null)
                {
                    _logger.LogDebug("Сохранение изображения рецепта. Размер: {FileSize}, Тип: {ContentType}",
                        request.Image.Length, request.Image.ContentType);

                    try
                    {
                        imageKey = await _imageStorage.SaveImageAsync(request.Image);
                        _logger.LogDebug("Изображение сохранено с ключом: {ImageKey}", imageKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при сохранении изображения рецепта");
                        throw;
                    }
                }

                var recipeIngredients = new List<RecipeIngredientCreateDto>();
                for (int i = 0; i < request.IngredientIds.Count; i++)
                {
                    recipeIngredients.Add(new RecipeIngredientCreateDto
                    {
                        IngredientId = request.IngredientIds[i],
                        Quantity = i < request.Quantities.Count ? request.Quantities[i] : null,
                        UnitId = i < request.UnitIds.Count ? request.UnitIds[i] : null
                    });
                }

                _logger.LogDebug("Создан список ингредиентов. Количество: {IngredientCount}",
                    recipeIngredients.Count);

                var createRecipeDto = new CreateRecipeDto
                {
                    Name = request.Name,
                    ShortDescription = request.ShortDescription,
                    FullDescription = request.FullDescription,
                    DishTypeId = request.DishTypeId,
                    RecipeIngredients = recipeIngredients,
                    RecipeEquipments = request.RecipeEquipments,
                    CookingTimeWithUser = request.CookingTimeWithUser,
                    CookingTimeWithoutUser = request.CookingTimeWithoutUser,
                    SpicinessLevel = request.SpicinessLevel,
                    DifficultyLevel = request.DifficultyLevel,
                    Servings = request.Servings
                };

                var user = await _userManager.FindByIdAsync(creatorId);
                var roles = await _userManager.GetRolesAsync(user);
                var isModerator = roles.Any(r => r == "Admin" || r == "Moderator");

                _logger.LogDebug("Пользователь является модератором: {IsModerator}", isModerator);

                RecipeDto recipeDto;
                try
                {
                    recipeDto = await _repository.AddAsync(createRecipeDto, creatorId, imageKey, isModerator);
                    _logger.LogInformation("Рецепт сохранен в базу данных. RecipeId: {RecipeId}", recipeDto.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении рецепта в базу данных");
                    throw;
                }

                try
                {
                    await CalculateNutritionInfoAsync(recipeDto);

                    await _repository.UpdateNutritionAsync(
                        recipeDto.Id,
                        recipeDto.TotalCalories,
                        recipeDto.TotalProteins,
                        recipeDto.TotalFats,
                        recipeDto.TotalCarbohydrates,
                        recipeDto.CaloriesPerServing,
                        recipeDto.ProteinsPerServing,
                        recipeDto.FatsPerServing,
                        recipeDto.CarbohydratesPerServing,
                        recipeDto.CaloriesPer100g,
                        recipeDto.ProteinsPer100g,
                        recipeDto.FatsPer100g,
                        recipeDto.CarbohydratesPer100g
                    );

                    _logger.LogInformation("Нутриенты рецепта обновлены. RecipeId: {RecipeId}", recipeDto.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при расчете и обновлении нутриентов рецепта. RecipeId: {RecipeId}",
                        recipeDto.Id);
                    // Не выбрасываем исключение, так как рецепт уже создан
                }

                return recipeDto;
            }
        }

        public async Task<RecipeDto?> UpdateRecipeAsync(int id, UpdateRecipeRequest request)
        {
            using (_logger.BeginScope(new { RecipeId = id, RecipeName = request.Name }))
            {
                _logger.LogInformation("Начало обновления рецепта");

                var recipeIngredients = new List<RecipeIngredientCreateDto>();
                for (int i = 0; i < request.IngredientIds.Count; i++)
                {
                    recipeIngredients.Add(new RecipeIngredientCreateDto
                    {
                        IngredientId = request.IngredientIds[i],
                        Quantity = i < request.Quantities.Count ? request.Quantities[i] : null,
                        UnitId = i < request.UnitIds.Count ? request.UnitIds[i] : null
                    });
                }

                var updateRecipeDto = new UpdateRecipeDto
                {
                    Id = id,
                    Name = request.Name,
                    ShortDescription = request.ShortDescription,
                    FullDescription = request.FullDescription,
                    DishTypeId = request.DishTypeId,
                    RecipeIngredients = recipeIngredients,
                    RecipeEquipments = request.RecipeEquipments,
                    CookingTimeWithUser = request.CookingTimeWithUser,
                    CookingTimeWithoutUser = request.CookingTimeWithoutUser,
                    SpicinessLevel = request.SpicinessLevel,
                    DifficultyLevel = request.DifficultyLevel,
                    Servings = request.Servings
                };

                var currentRecipe = await _repository.GetByIdAsync(id);
                var resetModeration = currentRecipe?.IsApproved == false;

                _logger.LogDebug("Требуется сброс модерации: {ResetModeration}", resetModeration);

                RecipeDto? recipeDto;
                try
                {
                    recipeDto = await _repository.UpdateAsync(id, updateRecipeDto, resetModeration);

                    if (recipeDto == null)
                    {
                        _logger.LogWarning("Рецепт не найден при обновлении. RecipeId: {RecipeId}", id);
                        return null;
                    }

                    _logger.LogInformation("Рецепт обновлен в базе данных. RecipeId: {RecipeId}", recipeDto.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обновлении рецепта в базе данных. RecipeId: {RecipeId}", id);
                    throw;
                }

                try
                {
                    await CalculateNutritionInfoAsync(recipeDto);

                    await _repository.UpdateNutritionAsync(
                        recipeDto.Id,
                        recipeDto.TotalCalories,
                        recipeDto.TotalProteins,
                        recipeDto.TotalFats,
                        recipeDto.TotalCarbohydrates,
                        recipeDto.CaloriesPerServing,
                        recipeDto.ProteinsPerServing,
                        recipeDto.FatsPerServing,
                        recipeDto.CarbohydratesPerServing,
                        recipeDto.CaloriesPer100g,
                        recipeDto.ProteinsPer100g,
                        recipeDto.FatsPer100g,
                        recipeDto.CarbohydratesPer100g
                    );

                    _logger.LogInformation("Нутриенты рецепта обновлены. RecipeId: {RecipeId}", recipeDto.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при расчете и обновлении нутриентов рецепта. RecipeId: {RecipeId}",
                        recipeDto.Id);
                    // Не выбрасываем исключение, так как рецепт уже обновлен
                }

                return recipeDto;
            }
        }

        public async Task DeleteRecipeAsync(int id)
        {
            using (_logger.BeginScope(new { RecipeId = id }))
            {
                _logger.LogWarning("Начало удаления рецепта");

                var recipe = await _repository.GetByIdAsync(id);
                if (recipe != null && !string.IsNullOrEmpty(recipe.ImagePath))
                {
                    _logger.LogDebug("Удаление изображения рецепта. ImagePath: {ImagePath}", recipe.ImagePath);
                    try
                    {
                        await _imageStorage.DeleteImageAsync(recipe.ImagePath);
                        _logger.LogDebug("Изображение рецепта удалено");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Ошибка при удалении изображения рецепта. RecipeId: {RecipeId}, ImagePath: {ImagePath}",
                            id, recipe.ImagePath);
                        // Продолжаем удаление рецепта даже если изображение не удалилось
                    }
                }

                try
                {
                    await _repository.DeleteAsync(id);
                    _logger.LogWarning("Рецепт удален из базы данных. RecipeId: {RecipeId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при удалении рецепта из базы данных. RecipeId: {RecipeId}", id);
                    throw;
                }
            }
        }

        public async Task<bool> UpdateRecipeImageAsync(int recipeId, IFormFile imageFile)
        {
            using (_logger.BeginScope(new { RecipeId = recipeId }))
            {
                _logger.LogInformation("Обновление изображения рецепта. Размер файла: {FileSize}, Тип: {ContentType}",
                    imageFile.Length, imageFile.ContentType);

                var oldRecipe = await _repository.GetByIdAsync(recipeId);
                if (oldRecipe == null)
                {
                    _logger.LogWarning("Рецепт не найден при обновлении изображения. RecipeId: {RecipeId}", recipeId);
                    return false;
                }

                string newImageKey;
                try
                {
                    newImageKey = await _imageStorage.SaveImageAsync(imageFile);
                    _logger.LogDebug("Новое изображение сохранено с ключом: {ImageKey}", newImageKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении нового изображения рецепта. RecipeId: {RecipeId}",
                        recipeId);
                    return false;
                }

                var result = await _repository.UpdateImagePathAsync(recipeId, newImageKey);

                if (result && !string.IsNullOrEmpty(oldRecipe.ImagePath))
                {
                    _logger.LogDebug("Удаление старого изображения рецепта. OldImagePath: {OldImagePath}",
                        oldRecipe.ImagePath);
                    try
                    {
                        await _imageStorage.DeleteImageAsync(oldRecipe.ImagePath);
                        _logger.LogDebug("Старое изображение удалено");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Ошибка при удалении старого изображения рецепта. RecipeId: {RecipeId}, OldImagePath: {OldImagePath}",
                            recipeId, oldRecipe.ImagePath);
                        // Продолжаем даже если старое изображение не удалилось
                    }
                }

                _logger.LogInformation("Изображение рецепта успешно обновлено. RecipeId: {RecipeId}, Result: {Result}",
                    recipeId, result);

                return result;
            }
        }

        public async Task<IEnumerable<RecipeDto>> GetRecentRecipesAsync(string? userId = null)
        {
            using (_logger.BeginScope(new { UserId = userId }))
            {
                _logger.LogDebug("Начало получения последних рецептов");

                var recipes = await _repository.GetRecentRecipesAsync();
                _logger.LogDebug("Получено {RecipeCount} последних рецептов из репозитория", recipes.Count());

                if (!string.IsNullOrEmpty(userId))
                {
                    var favoriteIds = await _favoriteRepository.GetFavoriteRecipeIdsAsync(userId);
                    foreach (var recipe in recipes)
                    {
                        recipe.IsFavorite = favoriteIds.Contains(recipe.Id);
                    }
                    _logger.LogDebug("Обновлены флаги избранного для пользователя");
                }

                foreach (var recipe in recipes)
                {
                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            recipe.ImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                            recipe.PreviewImageUrl = recipe.ImageUrl;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Ошибка при получении URL изображения рецепта. RecipeId: {RecipeId}, ImagePath: {ImagePath}",
                                recipe.Id, recipe.ImagePath);
                        }
                    }
                }

                await AddRatingsToRecipesAsync(recipes, userId);
                await AddCreatorInfoToRecipesAsync(recipes);

                _logger.LogInformation("Успешно возвращено {RecipeCount} последних рецептов", recipes.Count());
                return recipes;
            }
        }

        public async Task<IEnumerable<RecipeDto>> GetTopRecipesAsync(string? userId = null)
        {
            using (_logger.BeginScope(new { UserId = userId }))
            {
                _logger.LogDebug("Начало получения топовых рецептов");

                var recipes = await _repository.GetTopRecipesAsync();
                _logger.LogDebug("Получено {RecipeCount} топовых рецептов из репозитория", recipes.Count());

                if (!string.IsNullOrEmpty(userId))
                {
                    var favoriteIds = await _favoriteRepository.GetFavoriteRecipeIdsAsync(userId);
                    foreach (var recipe in recipes)
                    {
                        recipe.IsFavorite = favoriteIds.Contains(recipe.Id);
                    }
                    _logger.LogDebug("Обновлены флаги избранного для пользователя");
                }

                foreach (var recipe in recipes)
                {
                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            recipe.ImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                            recipe.PreviewImageUrl = recipe.ImageUrl;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Ошибка при получении URL изображения рецепта. RecipeId: {RecipeId}, ImagePath: {ImagePath}",
                                recipe.Id, recipe.ImagePath);
                        }
                    }
                }

                await AddRatingsToRecipesAsync(recipes, userId);
                await AddCreatorInfoToRecipesAsync(recipes);

                _logger.LogInformation("Успешно возвращено {RecipeCount} топовых рецептов", recipes.Count());
                return recipes;
            }
        }

        public async Task<IEnumerable<RecipeDto>> GetRecipesByIdsAsync(IEnumerable<int> recipeIds)
        {
            var ids = recipeIds.ToList();
            using (_logger.BeginScope(new { RecipeIds = ids }))
            {
                _logger.LogDebug("Начало получения рецептов по IDs. Количество: {RecipeCount}", ids.Count);

                var recipes = await _repository.GetByIdsAsync(ids);
                _logger.LogDebug("Получено {RecipeCount} рецептов из репозитория", recipes.Count());

                foreach (var recipe in recipes)
                {
                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            recipe.ImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                            recipe.PreviewImageUrl = recipe.ImageUrl;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Ошибка при получении URL изображения рецепта. RecipeId: {RecipeId}, ImagePath: {ImagePath}",
                                recipe.Id, recipe.ImagePath);
                        }
                    }
                }

                await AddRatingsToRecipesAsync(recipes);
                await AddCreatorInfoToRecipesAsync(recipes);

                _logger.LogInformation("Успешно возвращено {RecipeCount} рецептов", recipes.Count());
                return recipes;
            }
        }

        public async Task<bool> IsCreatorAsync(int recipeId, string userId)
        {
            _logger.LogDebug("Проверка, является ли пользователь создателем рецепта. RecipeId: {RecipeId}, UserId: {UserId}",
                recipeId, userId);

            var result = await _repository.IsCreatorAsync(recipeId, userId);

            _logger.LogDebug("Результат проверки создателя: {Result}. RecipeId: {RecipeId}, UserId: {UserId}",
                result, recipeId, userId);

            return result;
        }

        public async Task<bool> IsFavoriteAsync(string userId, int recipeId)
        {
            _logger.LogDebug("Проверка, добавлен ли рецепт в избранное. RecipeId: {RecipeId}, UserId: {UserId}",
                recipeId, userId);

            var result = await _favoriteRepository.IsFavoriteAsync(userId, recipeId);

            _logger.LogDebug("Результат проверки избранного: {Result}. RecipeId: {RecipeId}, UserId: {UserId}",
                result, recipeId, userId);

            return result;
        }

        private async Task AddRatingsToRecipesAsync(IEnumerable<RecipeDto> recipes, string? userId = null)
        {
            _logger.LogDebug("Добавление оценок к {RecipeCount} рецептам", recipes.Count());

            foreach (var recipe in recipes)
            {
                try
                {
                    var ratingSummary = await _ratingService.GetRatingSummaryAsync(recipe.Id, userId);
                    recipe.AverageRating = ratingSummary.AverageRating;
                    recipe.TotalRatings = ratingSummary.TotalRatings;
                    recipe.UserRating = ratingSummary.UserRating;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Ошибка при получении оценок для рецепта. RecipeId: {RecipeId}",
                        recipe.Id);
                }
            }
        }

        public async Task<IEnumerable<RecipeDto>> GetRecipesByUserIdAsync(string userId)
        {
            using (_logger.BeginScope(new { UserId = userId }))
            {
                _logger.LogDebug("Начало получения рецептов пользователя");

                var recipes = await _repository.GetRecipesByUserIdAsync(userId, includeAllStatuses: true);
                _logger.LogDebug("Получено {RecipeCount} рецептов пользователя из репозитория", recipes.Count());

                if (!string.IsNullOrEmpty(userId))
                {
                    var favoriteIds = await _favoriteRepository.GetFavoriteRecipeIdsAsync(userId);
                    foreach (var recipe in recipes)
                    {
                        recipe.IsFavorite = favoriteIds.Contains(recipe.Id);
                    }
                    _logger.LogDebug("Обновлены флаги избранного для пользователя");
                }

                foreach (var recipe in recipes)
                {
                    if (!string.IsNullOrEmpty(recipe.ImagePath))
                    {
                        try
                        {
                            recipe.ImageUrl = await _imageStorage.GetPreviewUrlAsync(recipe.ImagePath);
                            recipe.PreviewImageUrl = recipe.ImageUrl;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Ошибка при получении URL изображения рецепта. RecipeId: {RecipeId}, ImagePath: {ImagePath}",
                                recipe.Id, recipe.ImagePath);
                        }
                    }
                }

                await AddRatingsToRecipesAsync(recipes);
                await AddCreatorInfoToRecipesAsync(recipes);

                _logger.LogInformation("Успешно возвращено {RecipeCount} рецептов пользователя", recipes.Count());
                return recipes;
            }
        }

        public async Task<UserRecipesSummaryDto> GetUserRecipesSummaryAsync(string userId)
        {
            using (_logger.BeginScope(new { UserId = userId }))
            {
                _logger.LogDebug("Начало получения сводки рецептов пользователя");

                try
                {
                    var averageRating = await _repository.GetUserRecipesAverageRatingAsync(userId);
                    var recipesCount = await _repository.GetUserRecipesCountAsync(userId);
                    var totalRatings = await _repository.GetUserRecipesTotalRatingsAsync(userId);

                    _logger.LogInformation(
                        "Сводка рецептов пользователя получена. UserId: {UserId}, Рецептов: {RecipesCount}, Средняя оценка: {AverageRating}, Всего оценок: {TotalRatings}",
                        userId, recipesCount, averageRating, totalRatings);

                    return new UserRecipesSummaryDto
                    {
                        AverageRating = averageRating,
                        RecipesCount = recipesCount,
                        TotalRatings = totalRatings
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при получении сводки рецептов пользователя. UserId: {UserId}", userId);
                    throw;
                }
            }
        }

        private async Task AddCreatorInfoToRecipesAsync(IEnumerable<RecipeDto> recipes)
        {
            _logger.LogDebug("Добавление информации о создателях к {RecipeCount} рецептам", recipes.Count());

            var creatorIds = recipes.Select(r => r.CreatorId).Distinct().ToList();

            try
            {
                var creators = await _userManager.Users
                    .Where(u => creatorIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u);

                foreach (var recipe in recipes)
                {
                    if (creators.TryGetValue(recipe.CreatorId, out var creator))
                    {
                        recipe.CreatorUsername = creator.UserName;
                        if (!string.IsNullOrEmpty(creator.AvatarKey))
                        {
                            try
                            {
                                recipe.CreatorAvatarUrl = await _imageStorage.GetPreviewUrlAsync(creator.AvatarKey);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex,
                                    "Ошибка при получении аватара создателя. CreatorId: {CreatorId}, AvatarKey: {AvatarKey}",
                                    creator.Id, creator.AvatarKey);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении информации о создателях рецептов");
            }
        }

        public async Task<IEnumerable<RecipeModerationListDto>> GetRecipesForModerationAsync()
        {
            _logger.LogDebug("Начало получения рецептов для модерации");

            try
            {
                var recipes = await _repository.GetRecipesForModerationAsync();
                _logger.LogInformation("Получено {RecipeCount} рецептов для модерации", recipes.Count());
                return recipes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении рецептов для модерации");
                throw;
            }
        }

        public async Task<bool> ModerateRecipeAsync(ModerateRecipeDto dto, string moderatorId)
        {
            using (_logger.BeginScope(new { RecipeId = dto.RecipeId, ModeratorId = moderatorId }))
            {
                _logger.LogInformation("Начало модерации рецепта. Решение: {IsApproved}", dto.IsApproved);

                if (!dto.IsApproved && string.IsNullOrWhiteSpace(dto.RejectionComment))
                {
                    _logger.LogWarning("При отклонении рецепта необходимо указать комментарий. RecipeId: {RecipeId}",
                        dto.RecipeId);
                    throw new ApplicationException("При отклонении рецепта необходимо указать комментарий");
                }

                try
                {
                    var recipe = await _repository.GetRecipeForModerationAsync(dto.RecipeId);
                    if (recipe == null)
                        return false;

                    var result = await _repository.ModerateRecipeAsync(dto.RecipeId, dto.IsApproved, moderatorId, dto.RejectionComment);

                    if (result && dto.IsApproved)
                    {
                        await _userStatisticsService.IncrementRecipesPublishedAsync(recipe.CreatorId);
                        _logger.LogInformation("Увеличен счётчик опубликованных рецептов для пользователя {UserId}", recipe.CreatorId);
                    }

                    _logger.LogInformation(
                        "Модерация рецепта завершена. RecipeId: {RecipeId}, Результат: {Result}, Решение: {IsApproved}",
                        dto.RecipeId, result, dto.IsApproved);

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при модерации рецепта. RecipeId: {RecipeId}", dto.RecipeId);
                    throw;
                }
            }
        }

        public async Task<bool> ResubmitForModerationAsync(int recipeId)
        {
            using (_logger.BeginScope(new { RecipeId = recipeId }))
            {
                _logger.LogInformation("Повторная отправка рецепта на модерацию");

                try
                {
                    var result = await _repository.ResubmitForModerationAsync(recipeId);

                    _logger.LogInformation(
                        "Рецепт отправлен на повторную модерацию. RecipeId: {RecipeId}, Результат: {Result}",
                        recipeId, result);

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при повторной отправке рецепта на модерацию. RecipeId: {RecipeId}",
                        recipeId);
                    throw;
                }
            }
        }
    }
}