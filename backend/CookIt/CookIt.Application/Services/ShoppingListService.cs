using AutoMapper;
using CookIt.Core.Dtos.ShopingList;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;

public class ShoppingListService : IShoppingListService
{
    private readonly IShoppingListRepository _repository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserStatisticsService _userStatisticsService;
    private readonly IMapper _mapper;

    public ShoppingListService(IShoppingListRepository repository, IRecipeRepository recipeRepository,
            IUserStatisticsService userStatisticsService, IMapper mapper)
    {
        _repository = repository;
        _recipeRepository = recipeRepository;
        _userStatisticsService = userStatisticsService;
        _mapper = mapper;
    }

    public async Task<List<ShoppingListRecipeDto>> GetUserShoppingListAsync(string userId)
    {
        var items = await _repository.GetByUserIdAsync(userId);
        var result = new List<ShoppingListRecipeDto>();

        foreach (var item in items)
        {
            var recipe = item.Recipe;
            var originalServings = recipe.Servings ?? 1;
            var excludedIds = item.ExcludedIngredients.Select(e => e.IngredientId).ToHashSet();

            var ingredientDtos = recipe.RecipeIngredients
                .Select(ri => new ShoppingListIngredientDto
                {
                    IngredientId = ri.IngredientId,
                    Name = ri.Ingredient.Name,
                    Quantity = ri.Quantity ?? 0,
                    Unit = ri.Unit?.Name ?? "",
                    IsExcluded = excludedIds.Contains(ri.IngredientId),
                    IsByPiece = ri.Ingredient.IsByPiece,
                    ConversionToGrams = ri.Unit?.ConversionToGrams
                }).ToList();

            result.Add(new ShoppingListRecipeDto
            {
                Id = item.Id,
                RecipeId = recipe.Id,
                RecipeName = recipe.Name,
                Servings = item.Servings,
                OriginalServings = originalServings,
                Ingredients = ingredientDtos
            });
        }
        return result;
    }

    public async Task AddRecipeToListAsync(string userId, int recipeId, double servings)
    {
        var existing = (await _repository.GetByUserIdAsync(userId)).FirstOrDefault(sl => sl.RecipeId == recipeId);
        if (existing != null) return;

        var recipe = await _recipeRepository.GetByIdAsync(recipeId);
        if (recipe == null) throw new Exception("Рецепт не найден");

        if (recipe.CreatorId != userId)
        {
            await _userStatisticsService.IncrementShoppingListAddedAsync(userId);
        }

        var shoppingItem = new ShoppingList
        {
            UserId = userId,
            RecipeId = recipeId,
            Servings = servings,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(shoppingItem);
    }

    public async Task UpdateServingsAsync(int shoppingListId, double servings)
    {
        var item = await _repository.GetByIdAsync(shoppingListId);
        if (item == null) throw new Exception("Запись не найдена");
        item.Servings = servings;
        await _repository.UpdateAsync(item);
    }

    public async Task RemoveRecipeFromListAsync(int shoppingListId)
    {
        var item = await _repository.GetByIdAsync(shoppingListId);
        if (item == null) throw new Exception("Запись не найдена");

        var recipe = await _recipeRepository.GetByIdAsync(item.RecipeId);
        if (recipe != null && recipe.CreatorId != item.UserId)
        {
            await _userStatisticsService.DecrementShoppingListAddedAsync(item.UserId);
        }

        await _repository.DeleteAsync(item);
    }

    public async Task ToggleExcludeIngredientAsync(int shoppingListId, int ingredientId)
    {
        var item = await _repository.GetByIdAsync(shoppingListId);
        if (item == null) throw new Exception("Запись не найдена");

        var existing = await _repository.GetExcludedIngredientAsync(shoppingListId, ingredientId);
        if (existing == null)
        {
            await _repository.AddExcludedIngredientAsync(new ShoppingListExcludedIngredient
            {
                ShoppingListId = shoppingListId,
                IngredientId = ingredientId
            });
        }
        else
        {
            await _repository.RemoveExcludedIngredientAsync(existing);
        }
    }
}