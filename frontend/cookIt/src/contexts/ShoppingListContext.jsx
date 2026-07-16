import React, { createContext, useState, useContext, useEffect } from 'react';
import { API_BASE_URL } from '../config/settings';
import { useAuth } from './AuthContext';

const ShoppingListContext = createContext();

export const ShoppingListProvider = ({ children }) => {
  const { accessToken } = useAuth();
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchShoppingList = async () => {
    if (!accessToken) return;
    try {
      const res = await fetch(`${API_BASE_URL}/api/shoppinglist`, {
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      const data = await res.json();
      setRecipes(data);
    } catch (err) {
      console.error('Ошибка загрузки списка покупок', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (accessToken) {
      fetchShoppingList();
    } else {
      setLoading(false); 
    }
  }, [accessToken]);

  const addRecipeToList = async (recipe, servings = recipe.servings || 1) => {
    if (!accessToken) return;
    try {
      await fetch(`${API_BASE_URL}/api/shoppinglist?recipeId=${recipe.id}&servings=${servings}`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      await fetchShoppingList();
    } catch (err) {
      console.error('Ошибка добавления рецепта в список', err);
    }
  };

  const removeRecipe = async (shoppingListId) => {
    if (!accessToken) return;
    try {
      await fetch(`${API_BASE_URL}/api/shoppinglist/${shoppingListId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      await fetchShoppingList();
    } catch (err) {
      console.error('Ошибка удаления рецепта из списка', err);
    }
  };

  const updateServings = async (shoppingListId, newServings) => {
    if (!accessToken) return;
    try {
      await fetch(`${API_BASE_URL}/api/shoppinglist/${shoppingListId}/servings?servings=${newServings}`, {
        method: 'PUT',
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      await fetchShoppingList();
    } catch (err) {
      console.error('Ошибка обновления порций', err);
    }
  };

  const toggleExcludeIngredient = async (shoppingListId, ingredientId) => {
    if (!accessToken) return;
    try {
      await fetch(`${API_BASE_URL}/api/shoppinglist/${shoppingListId}/exclude-ingredient?ingredientId=${ingredientId}`, {
        method: 'PATCH',
        headers: { Authorization: `Bearer ${accessToken}` }
      });
      await fetchShoppingList();
    } catch (err) {
      console.error('Ошибка переключения исключения ингредиента', err);
    }
  };

  const isRecipeInList = (recipeId) => {
    return recipes.some(item => item.recipeId === recipeId);
  };

  const removeRecipeByRecipeId = async (recipeId) => {
    const item = recipes.find(r => r.recipeId === recipeId);
    if (item) {
      await removeRecipe(item.id); 
    }
  };

  return (
    <ShoppingListContext.Provider value={{
      recipes,
      loading,
      addRecipeToList,
      removeRecipe,
      updateServings,
      toggleExcludeIngredient,
      isRecipeInList,          
      removeRecipeByRecipeId,  
    }}>
      {children}
    </ShoppingListContext.Provider>
  );
};

export const useShoppingList = () => useContext(ShoppingListContext);