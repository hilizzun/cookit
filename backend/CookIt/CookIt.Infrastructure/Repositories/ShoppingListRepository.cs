using CookIt.Core.Entities;
using CookIt.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ShoppingListRepository : IShoppingListRepository
{
    private readonly CookItContext _context;

    public ShoppingListRepository(CookItContext context)
    {
        _context = context;
    }

    public async Task<ShoppingList?> GetByIdAsync(int id)
    {
        return await _context.ShoppingLists
            .Include(sl => sl.Recipe)
                .ThenInclude(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .Include(sl => sl.ExcludedIngredients)
            .FirstOrDefaultAsync(sl => sl.Id == id);
    }

    public async Task<List<ShoppingList>> GetByUserIdAsync(string userId)
    {
        return await _context.ShoppingLists
            .Include(sl => sl.Recipe)
                .ThenInclude(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
            .Include(sl => sl.Recipe)
                .ThenInclude(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
            .Include(sl => sl.ExcludedIngredients)
            .Where(sl => sl.UserId == userId)
            .ToListAsync();
    }

    public async Task<ShoppingList> AddAsync(ShoppingList shoppingList)
    {
        shoppingList.CreatedAt = DateTime.UtcNow;
        shoppingList.UpdatedAt = DateTime.UtcNow;
        await _context.ShoppingLists.AddAsync(shoppingList);
        await _context.SaveChangesAsync();
        return shoppingList;
    }

    public async Task UpdateAsync(ShoppingList shoppingList)
    {
        shoppingList.UpdatedAt = DateTime.UtcNow;
        _context.ShoppingLists.Update(shoppingList);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ShoppingList shoppingList)
    {
        _context.ShoppingLists.Remove(shoppingList);
        await _context.SaveChangesAsync();
    }

    public async Task<ShoppingListExcludedIngredient?> GetExcludedIngredientAsync(int shoppingListId, int ingredientId)
    {
        return await _context.ShoppingListExcludedIngredients
            .FirstOrDefaultAsync(e => e.ShoppingListId == shoppingListId && e.IngredientId == ingredientId);
    }

    public async Task AddExcludedIngredientAsync(ShoppingListExcludedIngredient excluded)
    {
        await _context.ShoppingListExcludedIngredients.AddAsync(excluded);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveExcludedIngredientAsync(ShoppingListExcludedIngredient excluded)
    {
        _context.ShoppingListExcludedIngredients.Remove(excluded);
        await _context.SaveChangesAsync();
    }
}