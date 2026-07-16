import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import RecipeForm from '../components/RecipeForm';
import { API_BASE_URL } from '../config/settings';
import { Button } from '../components/ui/button';
import { Loader2 } from 'lucide-react';

const RecipeEditPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const { accessToken } = useAuth();
  const [recipe, setRecipe] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchRecipe = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_BASE_URL}/api/recipes/${id}`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (response.status === 401) {
          navigate('/login');
          return;
        }
        if (!response.ok) throw new Error('Ошибка загрузки рецепта');
        const data = await response.json();
        setRecipe(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };
    fetchRecipe();
  }, [id, accessToken, navigate]);

  const handleSave = async (formattedData) => {
    try {
      setError(null);
      const formData = new FormData();
      formData.append('Id', id.toString());
      formData.append('Name', formattedData.name);
      formData.append('ShortDescription', formattedData.shortDescription);
      formData.append('FullDescription', formattedData.fullDescription);
      formData.append('DishTypeId', formattedData.dishTypeId.toString());

      formattedData.recipeIngredients.forEach((ingredient, index) => {
        formData.append(`IngredientIds[${index}]`, ingredient.ingredientId.toString());
        if (ingredient.quantity !== null && ingredient.quantity !== '') {
          formData.append(`Quantities[${index}]`, ingredient.quantity.toString());
        } else {
          formData.append(`Quantities[${index}]`, '');
        }
        if (ingredient.unitId !== null && ingredient.unitId !== '') {
          formData.append(`UnitIds[${index}]`, ingredient.unitId.toString());
        } else {
          formData.append(`UnitIds[${index}]`, '');
        }
      });

      formattedData.recipeEquipments.forEach((equipmentId, index) => {
        formData.append(`RecipeEquipments[${index}]`, equipmentId.toString());
      });

      formData.append('CookingTimeWithUser', formattedData.cookingTimeWithUser.toString());
      formData.append('CookingTimeWithoutUser', formattedData.cookingTimeWithoutUser.toString());
      formData.append('SpicinessLevel', formattedData.spicinessLevel.toString());
      formData.append('DifficultyLevel', formattedData.difficultyLevel.toString());

      if (formattedData.servings) {
        formData.append('Servings', formattedData.servings.toString());
      }

      if (formattedData.image) {
        formData.append('Image', formattedData.image);
      }

      const response = await fetch(`${API_BASE_URL}/api/recipes/${id}`, {
        method: 'PUT',
        headers: { Authorization: `Bearer ${accessToken}` },
        body: formData,
      });

      if (response.status === 401) {
        navigate('/login');
        return;
      }

      if (!response.ok) {
        let errorMessage = 'Ошибка при сохранении рецепта';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorMessage;
        } catch (e) {}
        throw new Error(errorMessage);
      }

      navigate(`/recipes/${id}`);
    } catch (err) {
      setError(err.message);
      console.error('Ошибка при сохранении:', err);
    }
  };

  const handleCancel = () => {
    navigate(`/recipes/${id}`);
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
        <p className="text-red-500 mb-4">Ошибка: {error}</p>
        <Button onClick={() => navigate('/recipes')}>Назад к рецептам</Button>
      </div>
    );
  }

  if (!recipe) {
    return <div className="text-center py-12">Рецепт не найден</div>;
  }

  return (
    <div className="pr-90">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Редактирование рецепта</h1>
        <p className="text-muted-foreground mt-2 text-base">
          Внесите необходимые изменения в рецепт
        </p>
      </div>
      {error && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded text-center">
          {error}
        </div>
      )}
      <RecipeForm recipe={recipe} onSave={handleSave} onCancel={handleCancel} />
    </div>
  );
};

export default RecipeEditPage;