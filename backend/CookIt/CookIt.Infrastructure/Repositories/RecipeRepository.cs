using AutoMapper;
using CookIt.Core.Dtos.Recipes;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CookIt.Infrastructure.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly CookItContext _context;
        private readonly IMapper _mapper;

        public RecipeRepository(CookItContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Recipe>> GetAllApprovedAsync()
        {
            return await _context.Recipes
                .Where(r => r.IsApproved == true)
                .ToListAsync();
        }

        public async Task<(IEnumerable<RecipeDto> Recipes, int TotalCount)> GetAllAsync(
            RecipeFilterDto? filter = null,
            string? userId = null,
            bool includeUnapproved = false)
        {
            var query = _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Include(r => r.ApprovedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter?.SearchText))
            {
                query = query.Where(r =>
                    EF.Functions.ToTsVector("russian",
                        r.Name + " " + r.ShortDescription + " " + r.FullDescription)
                        .Matches(EF.Functions.WebSearchToTsQuery("russian", filter.SearchText)));
            }

            if (!includeUnapproved)
            {
                query = query.Where(r => r.IsApproved == true);
            }

            if (filter != null)
            {
                if (filter.MinCookingTimeWithUser.HasValue)
                    query = query.Where(r => r.CookingTimeWithUser >= filter.MinCookingTimeWithUser.Value);
                if (filter.MaxCookingTimeWithUser.HasValue)
                    query = query.Where(r => r.CookingTimeWithUser <= filter.MaxCookingTimeWithUser.Value);

                if (filter.MinCookingTimeWithoutUser.HasValue)
                    query = query.Where(r => r.CookingTimeWithoutUser >= filter.MinCookingTimeWithoutUser.Value);
                if (filter.MaxCookingTimeWithoutUser.HasValue)
                    query = query.Where(r => r.CookingTimeWithoutUser <= filter.MaxCookingTimeWithoutUser.Value);

                if (filter.MinSpicinessLevel.HasValue)
                    query = query.Where(r => r.SpicinessLevel >= filter.MinSpicinessLevel.Value);
                if (filter.MaxSpicinessLevel.HasValue)
                    query = query.Where(r => r.SpicinessLevel <= filter.MaxSpicinessLevel.Value);

                if (filter.MinDifficultyLevel.HasValue)
                    query = query.Where(r => r.DifficultyLevel >= filter.MinDifficultyLevel.Value);
                if (filter.MaxDifficultyLevel.HasValue)
                    query = query.Where(r => r.DifficultyLevel <= filter.MaxDifficultyLevel.Value);

                if (filter.DishTypeIds != null && filter.DishTypeIds.Any())
                    query = query.Where(r => filter.DishTypeIds.Contains(r.DishTypeId));

                if (filter.EquipmentIds != null && filter.EquipmentIds.Any())
                    query = query.Where(r => r.RecipeEquipments.Any(re => filter.EquipmentIds.Contains(re.EquipmentId)));

                if (filter.IngredientIds != null && filter.IngredientIds.Any())
                    query = query.Where(r => r.RecipeIngredients.Any(ri => filter.IngredientIds.Contains(ri.IngredientId)));

                if (filter.IsApproved.HasValue)
                    query = query.Where(r => r.IsApproved == filter.IsApproved.Value);
            }

            int totalCount = await query.CountAsync();

            query = query.Skip((filter.PageNumber - 1) * filter.PageSize)
                        .Take(filter.PageSize);

            var recipes = await query.ToListAsync();
            var recipeDtos = _mapper.Map<IEnumerable<RecipeDto>>(recipes).ToList();

            if (!string.IsNullOrEmpty(userId))
            {
                var favoriteIds = await _context.UserFavorites
                    .Where(uf => uf.UserId == userId)
                    .Select(uf => uf.RecipeId)
                    .ToListAsync();

                foreach (var dto in recipeDtos)
                {
                    dto.IsFavorite = favoriteIds.Contains(dto.Id);
                }
            }

            return (recipeDtos, totalCount);
        }

        public async Task<RecipeDto?> GetByIdAsync(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .FirstOrDefaultAsync(r => r.Id == id);

            return recipe == null ? null : _mapper.Map<RecipeDto>(recipe);
        }

        public async Task<RecipeDto> AddAsync(CreateRecipeDto recipeDto, string creatorId, string? imageKey = null, bool isModerator = false)
        {
            var equipmentList = await _context.Equipments
                .Where(e => recipeDto.RecipeEquipments.Contains(e.Id))
                .ToListAsync();

            var dishType = await _context.DishTypes
                .FirstOrDefaultAsync(dt => dt.Id == recipeDto.DishTypeId);

            var recipe = _mapper.Map<Recipe>(recipeDto);
            recipe.DishType = dishType;

            recipe.RecipeEquipments = equipmentList
                .Select(e => new RecipeEquipment { EquipmentId = e.Id }).ToList();

            recipe.RecipeIngredients = recipeDto.RecipeIngredients
                .Select(ri => new RecipeIngredient
                {
                    IngredientId = ri.IngredientId,
                    Quantity = ri.Quantity,
                    UnitId = ri.UnitId
                }).ToList();

            recipe.Servings = recipeDto.Servings;

            recipe.CreatorId = creatorId;
            recipe.ImagePath = imageKey;

            recipe.IsApproved = isModerator ? true : (bool?)null;
            if (isModerator)
            {
                recipe.ApprovedAt = DateTime.UtcNow;
                recipe.ApprovedById = creatorId;
            }

            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();

            var loadedRecipe = await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .FirstOrDefaultAsync(r => r.Id == recipe.Id);

            return _mapper.Map<RecipeDto>(loadedRecipe);
        }

        public async Task<RecipeDto?> UpdateAsync(int id, UpdateRecipeDto recipeDto, bool resetModeration = false)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeEquipments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return null;

            _mapper.Map(recipeDto, recipe);

            recipe.DishType = await _context.DishTypes.FindAsync(recipeDto.DishTypeId);

            _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
            var newIngredients = recipeDto.RecipeIngredients
                .Select(ri => new RecipeIngredient
                {
                    RecipeId = recipe.Id,
                    IngredientId = ri.IngredientId,
                    Quantity = ri.Quantity,
                    UnitId = ri.UnitId
                }).ToList();

            _context.RecipeEquipments.RemoveRange(recipe.RecipeEquipments);
            var newEquipments = recipeDto.RecipeEquipments
                .Select(equipmentId => new RecipeEquipment { RecipeId = recipe.Id, EquipmentId = equipmentId })
                .ToList();
            recipe.RecipeEquipments = newEquipments;
            recipe.RecipeIngredients = newIngredients;
            recipe.Servings = recipeDto.Servings;

            // Если рецепт был отклонен и автор его обновляет - сбрасываем статус модерации
            if (resetModeration && recipe.IsApproved == false)
            {
                recipe.IsApproved = null;
                recipe.ApprovedAt = null;
                recipe.ApprovedById = null;
                // Комментарий отклонения остается
            }

            await _context.SaveChangesAsync();

            recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Include(r => r.DishType)
                .FirstOrDefaultAsync(r => r.Id == recipe.Id);

            return _mapper.Map<RecipeDto>(recipe);
        }

        public async Task DeleteAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateImagePathAsync(int id, string imageKey)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return false;

            recipe.ImagePath = imageKey;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RecipeDto>> GetRecentRecipesAsync()
        {
            var recipes = await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .OrderByDescending(r => r.Id)
                .Take(100)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<RecipeDto>>(recipes);
        }

        public async Task<IEnumerable<RecipeDto>> GetTopRecipesAsync()
        {
            var recipes = await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Include(r => r.Ratings)
                .Select(r => new
                {
                    Recipe = r,
                    AverageRating = r.Ratings.Any() ? r.Ratings.Average(rr => rr.Value) : 0,
                    TotalRatings = r.Ratings.Count
                })
                .Where(x => x.AverageRating > 0)
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.TotalRatings)
                .Take(100)
                .Select(x => x.Recipe)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<RecipeDto>>(recipes);
        }

        public async Task<bool> IsCreatorAsync(int recipeId, string userId)
        {
            return await _context.Recipes
                .AnyAsync(r => r.Id == recipeId && r.CreatorId == userId);
        }

        public async Task<IEnumerable<RecipeDto>> GetByIdsAsync(IEnumerable<int> recipeIds)
        {
            var recipes = await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Where(r => recipeIds.Contains(r.Id))
                .ToListAsync();

            return _mapper.Map<IEnumerable<RecipeDto>>(recipes);
        }

        // Получение рецептов пользователя с учетом статуса модерации
        public async Task<IEnumerable<RecipeDto>> GetRecipesByUserIdAsync(string userId, bool includeAllStatuses = false)
        {
            var query = _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Include(r => r.Ratings)
                .Where(r => r.CreatorId == userId);

            // Если не нужно включать все статусы, показываем только одобренные
            if (!includeAllStatuses)
            {
                query = query.Where(r => r.IsApproved == true);
            }

            var recipes = await query
                .OrderByDescending(r => r.Ratings.Average(rr => (double?)rr.Value) ?? 0)
                .ThenByDescending(r => r.Ratings.Count)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<RecipeDto>>(recipes);
        }

        public async Task<double> GetUserRecipesAverageRatingAsync(string userId)
        {
            var recipesWithRatings = await _context.Recipes
                .Where(r => r.CreatorId == userId)
                .Select(r => new
                {
                    RecipeId = r.Id,
                    AverageRating = r.Ratings.Any() ? r.Ratings.Average(rr => rr.Value) : (double?)null
                })
                .Where(r => r.AverageRating.HasValue)
                .ToListAsync();

            if (!recipesWithRatings.Any())
                return 0;

            return Math.Round(recipesWithRatings.Average(r => r.AverageRating.Value), 1);
        }

        public async Task<int> GetUserRecipesCountAsync(string userId)
        {
            return await _context.Recipes
                .CountAsync(r => r.CreatorId == userId);
        }

        public async Task<int> GetUserRecipesTotalRatingsAsync(string userId)
        {
            return await _context.Recipes
                .Where(r => r.CreatorId == userId)
                .SelectMany(r => r.Ratings)
                .CountAsync();
        }

        public async Task UpdateNutritionAsync(
            int recipeId,
            double totalCalories,
            double totalProteins,
            double totalFats,
            double totalCarbs,
            double caloriesPerServing,
            double proteinsPerServing,
            double fatsPerServing,
            double carbsPerServing,
            double caloriesPer100g,
            double proteinsPer100g,
            double fatsPer100g,
            double carbsPer100g)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe != null)
            {
                recipe.TotalCalories = totalCalories;
                recipe.TotalProteins = totalProteins;
                recipe.TotalFats = totalFats;
                recipe.TotalCarbohydrates = totalCarbs;
                recipe.CaloriesPerServing = caloriesPerServing;
                recipe.ProteinsPerServing = proteinsPerServing;
                recipe.FatsPerServing = fatsPerServing;
                recipe.CarbohydratesPerServing = carbsPerServing;
                recipe.CaloriesPer100g = caloriesPer100g;
                recipe.ProteinsPer100g = proteinsPer100g;
                recipe.FatsPer100g = fatsPer100g;
                recipe.CarbohydratesPer100g = carbsPer100g;

                await _context.SaveChangesAsync();
            }
        }

        // Получение рецептов для модерации (только с IsApproved == null)
        public async Task<IEnumerable<RecipeModerationListDto>> GetRecipesForModerationAsync()
        {
            var recipes = await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Where(r => r.IsApproved == null)
                .ToListAsync();

            var result = new List<RecipeModerationListDto>();

            foreach (var recipe in recipes)
            {
                var author = await _context.Users.FindAsync(recipe.CreatorId);

                result.Add(new RecipeModerationListDto
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    AuthorName = author?.FullName ?? author?.UserName ?? "Неизвестный автор",
                    AuthorId = recipe.CreatorId,
                    ShortDescription = recipe.ShortDescription,
                    DishTypeName = recipe.DishType?.Name ?? "Не указано"
                });
            }

            return result;
        }

        // Получение рецепта для модерации
        public async Task<Recipe?> GetRecipeForModerationAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.DishType)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.RecipeEquipments)
                    .ThenInclude(re => re.Equipment)
                .Include(r => r.ApprovedBy)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsApproved == null);
        }

        // Модерация рецепта
        public async Task<bool> ModerateRecipeAsync(int recipeId, bool isApproved, string moderatorId, string? rejectionComment = null)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
                return false;

            recipe.IsApproved = isApproved;
            recipe.ApprovedAt = DateTime.UtcNow;
            recipe.ApprovedById = moderatorId;

            if (isApproved)
            {
                recipe.RejectionComment = null;
            }
            else
            {
                recipe.RejectionComment = rejectionComment;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Повторная отправка на модерацию
        public async Task<bool> ResubmitForModerationAsync(int recipeId)
        {
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
                return false;

            // Сбрасываем статус модерации, но оставляем комментарий
            recipe.IsApproved = null;
            recipe.ApprovedAt = null;
            recipe.ApprovedById = null;
            // Комментарий отклонения остается для информации модератору

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetIngredientIdsByRecipeIdAsync(int recipeId)
        {
            return await _context.RecipeIngredients
                .Where(ri => ri.RecipeId == recipeId)
                .Select(ri => ri.IngredientId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRandomApprovedRecipesAsync(int count)
        {
            var random = new Random();
            var recipes = await _context.Recipes
                .Where(r => r.IsApproved == true)
                .ToListAsync();
            return recipes.OrderBy(r => random.Next()).Take(count);
        }

    }

}
