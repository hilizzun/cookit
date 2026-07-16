import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import RecipeForm from '../components/RecipeForm';
import { API_BASE_URL } from '../config/settings';

const RecipeCreatePage = () => {
  const navigate = useNavigate();
  const { accessToken } = useAuth();
  const [error, setError] = React.useState(null);

  const handleSave = async (formattedData) => {
    try {
      setError(null);
      const formData = new FormData();
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

      const response = await fetch(`${API_BASE_URL}/api/recipes`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}` },
        body: formData,
      });

      if (response.status === 401) {
        navigate('/login');
        return;
      }

      if (!response.ok) {
        let errorMessage = 'Ошибка при создании рецепта';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorMessage;
        } catch (e) {}
        throw new Error(errorMessage);
      }

      navigate('/recipes');
    } catch (err) {
      setError(err.message);
      console.error('Ошибка при создании:', err);
    }
  };

  const handleCancel = () => {
    navigate('/recipes');
  };

  return (
    <div className="pr-90">
      <div className="mb-8">
        <h1 className="text-3xl font-bold tracking-tight">Добавление рецепта</h1>
        <p className="text-muted-foreground mt-2 text-base">
          Заполните информацию о новом блюде
        </p>
      </div>
      {error && (
        <div className="mb-4 p-3 bg-red-100 text-red-700 rounded text-center">
          {error}
        </div>
      )}
      <RecipeForm onSave={handleSave} onCancel={handleCancel} />
    </div>
  );
};

export default RecipeCreatePage;