import { Link } from 'react-router-dom';
import { FaHeart, FaRegHeart } from "react-icons/fa";
import { FaStar } from "react-icons/fa";
import { Card, CardContent } from "./ui/card";
import { Button } from "./ui/button";
import { useAuth } from '../contexts/AuthContext';
import { useShoppingList } from '../contexts/ShoppingListContext';
import { pdf } from '@react-pdf/renderer';
import RecipePDF from './RecipePDF';
import { ShoppingCart, FileDown } from 'lucide-react';
import { useState } from 'react';

const RecipeCard = ({ recipe, onToggleFavorite }) => {
  const { accessToken } = useAuth();
  const { addRecipeToList, removeRecipeByRecipeId, isRecipeInList } = useShoppingList();
  const [exporting, setExporting] = useState(false);
  const isInShoppingList = isRecipeInList(recipe.id);

  const handleToggleShoppingList = () => {
    if (isInShoppingList) {
      removeRecipeByRecipeId(recipe.id);
    } else {
      addRecipeToList(recipe, recipe.servings || 1);
    }
  };

  const handleExportPDF = async () => {
    if (exporting) return;
    setExporting(true);
    try {
      const blob = await pdf(<RecipePDF recipe={recipe} />).toBlob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${recipe.name}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Ошибка при создании PDF:', error);
    } finally {
      setExporting(false);
    }
  };

  return (
    <Card className="border-0 shadow-none h-full flex flex-col group animate-fade-up">
      <Link to={`/recipes/${recipe.id}`} className="flex-1 flex flex-col">
        {recipe.imageUrl && (
          <div className="relative mb-3 h-48 w-full overflow-hidden rounded-lg">
            <img
              src={recipe.previewImageUrl || recipe.imageUrl}
              alt={recipe.name}
              className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
              onError={(e) => {
                e.target.style.display = 'none';
              }}
            />
            <div className="absolute top-2 left-2 bg-white/90 backdrop-blur-sm px-2 py-1 rounded-full shadow-sm flex items-center text-sm">
              <FaStar className="text-yellow-500 mr-1 h-3 w-3" />
              <span className="font-semibold text-gray-800">
                {recipe.averageRating?.toFixed(1) || '0.0'}
              </span>
              <span className="text-xs text-gray-500 ml-1">
                ({recipe.totalRatings || 0})
              </span>
            </div>
            <button
              className="absolute top-2 right-2 bg-white/90 backdrop-blur-sm p-2 rounded-full shadow-sm hover:bg-white transition-colors"
              onClick={(e) => {
                e.preventDefault();
                onToggleFavorite?.(recipe.id, recipe.isFavorite);
              }}
            >
              {recipe.isFavorite ? (
                <FaHeart className="text-red-500 h-4 w-4" />
              ) : (
                <FaRegHeart className="text-gray-600 h-4 w-4" />
              )}
            </button>
            <button
              className="absolute top-2 right-12 bg-white/90 backdrop-blur-sm p-2 rounded-full shadow-sm hover:bg-white transition-colors"
              onClick={(e) => {
                e.preventDefault();
                handleToggleShoppingList();
              }}
              title={isInShoppingList ? "Убрать из списка покупок" : "Добавить в список покупок"}
            >
              <ShoppingCart className={`h-4 w-4 ${isInShoppingList ? 'text-violet-600 fill-violet-600' : 'text-gray-600'}`} />
            </button>
            <button
              className="absolute top-2 right-22 bg-white/90 backdrop-blur-sm p-2 rounded-full shadow-sm hover:bg-white transition-colors"
              onClick={(e) => {
                e.preventDefault();
                handleExportPDF();
              }}
              title="Скачать PDF рецепта"
              disabled={exporting}
            >
              <FileDown className="h-4 w-4 text-gray-600" />
            </button>
          </div>
        )}
        <CardContent className="p-0 flex-1 flex flex-col">
          <h3 className="font-medium text-gray-900 text-base mb-1 line-clamp-1 group-hover:text-primary transition-colors duration-200">
            {recipe.name || "Без названия"}
          </h3>
          <p className="text-sm text-gray-500 line-clamp-2">
            {recipe.shortDescription || "Описание отсутствует"}
          </p>
        </CardContent>
      </Link>
    </Card>
  );
};

export default RecipeCard;