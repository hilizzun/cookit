import React, { useState, useEffect } from "react";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import RecipeCard from "../components/RecipeCard";
import { API_BASE_URL } from '../config/settings';
import { Button } from "../components/ui/button";
import { Loader2 } from "lucide-react";

export default function TopRecipesPage() {
  const { accessToken } = useAuth();
  const navigate = useNavigate();
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchTopRecipes = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/top`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (response.status === 401) {
        navigate("/login");
        return;
      }
      if (!response.ok) throw new Error(`Ошибка: ${response.status}`);
      const data = await response.json();
      setRecipes(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error(err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTopRecipes();
  }, [accessToken, navigate]);

  const handleToggleFavorite = async (recipeId, isFavorite) => {
    try {
      const url = `${API_BASE_URL}/api/favorites/${recipeId}`;
      const method = isFavorite ? 'DELETE' : 'POST';
      const response = await fetch(url, {
        method: method,
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка при обновлении избранного');
      setRecipes(prev =>
        prev.map(r => (r.id === recipeId ? { ...r, isFavorite: !isFavorite } : r))
      );
    } catch (err) {
      console.error(err);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500 mb-4">{error}</p>
        <Button onClick={fetchTopRecipes} variant="outline">
          Повторить
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Лучшее
        </h1>
        <p className="text-muted-foreground mt-3 text-base">
          Самые популярные рецепты по рейтингу
        </p>
      </div>

      {recipes.length > 0 ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-6 gap-6 auto-rows-fr">
          {recipes.map((recipe, index) => {
            const sizePattern = [4, 2, 2, 2, 2, 3, 3, 2, 4];
            const size = sizePattern[index % sizePattern.length];
            const sizeClasses = {
              2: "lg:col-span-2",
              3: "lg:col-span-3",
              4: "lg:col-span-4",
            };

            return (
              <div key={recipe.id} className={sizeClasses[size]}>
                <RecipeCard
                  recipe={recipe}
                  onToggleFavorite={handleToggleFavorite}
                />
              </div>
            );
          })}
        </div>
      ) : (
        <div className="text-center py-12">
          <p className="text-muted-foreground mb-4">Рецептов не найдено</p>
          <Button onClick={fetchTopRecipes} variant="outline">
            Обновить
          </Button>
        </div>
      )}
    </div>
  );
}