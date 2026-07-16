using CookIt.Core.Entities;
using CookIt.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly CookItContext _context;

    public FavoriteRepository(CookItContext context)
    {
        _context = context;
    }

    public async Task AddFavoriteAsync(string userId, int recipeId)
    {
        if (await _context.UserFavorites.FindAsync(userId, recipeId) == null)
        {
            _context.UserFavorites.Add(new UserFavorite
            {
                UserId = userId,
                RecipeId = recipeId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveFavoriteAsync(string userId, int recipeId)
    {
        var favorite = await _context.UserFavorites.FindAsync(userId, recipeId);
        if (favorite != null)
        {
            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsFavoriteAsync(string userId, int recipeId)
    {
        return await _context.UserFavorites
            .AnyAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);
    }

    public async Task<IEnumerable<int>> GetFavoriteRecipeIdsAsync(string userId)
    {
        return await _context.UserFavorites
            .Where(uf => uf.UserId == userId)
            .Select(uf => uf.RecipeId)
            .ToListAsync();
    }
}