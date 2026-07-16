import React, { useState, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from './ui/dialog';
import { Button } from './ui/button';
import { Loader2 } from 'lucide-react';
import { API_BASE_URL } from '../config/settings';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import DomeGallery from './DomeGallery';

export const RecipeDomeModal = ({ isOpen, onClose }) => {
  const { accessToken } = useAuth();
  const navigate = useNavigate();
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedRecipe, setSelectedRecipe] = useState(null);
  const [detailOpen, setDetailOpen] = useState(false);

  useEffect(() => {
    if (isOpen) {
      fetchRecipesForDome();
    } else {
      setRecipes([]);
      setSelectedRecipe(null);
      setDetailOpen(false);
    }
  }, [isOpen]);

  const fetchRecipesForDome = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/random-wheel?count=30`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка загрузки рецептов');
      const data = await response.json();
      setRecipes(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleImageClick = (recipeId) => {
    const recipe = recipes.find(r => String(r.id) === String(recipeId));
    if (recipe) {
      setSelectedRecipe(recipe);
      setDetailOpen(true);
    }
  };

  const handleGoToRecipe = () => {
    if (selectedRecipe) {
      onClose();
      setDetailOpen(false);
      navigate(`/recipes/${selectedRecipe.id}`);
    }
  };

  const galleryImages = recipes.map(recipe => ({
    src: recipe.previewImageUrl,
    alt: recipe.name,
    recipeId: recipe.id
  }));

  return (
    <>
      <Dialog open={isOpen} onOpenChange={onClose}>
        <DialogContent className="max-w-6xl bg-white max-h-[720px] h-full flex flex-col p-0 overflow-hidden border-0">
          {loading ? (
            <div className="flex-1 flex justify-center items-center">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : (
            <div className="flex-1 w-full h-full relative">
              <DomeGallery
                images={galleryImages}
                fit={0.55}
                minRadius={200}
                maxVerticalRotationDeg={7}
                segments={28}
                dragDampening={2.6}
                grayscale={false}
                onImageClick={handleImageClick}
              />
              <div className="absolute top-4 left-1/2 transform -translate-x-1/2 bg-black/50 backdrop-blur-sm px-6 py-2 rounded-full text-white text-xl font-semibold whitespace-nowrap z-10 pointer-events-none">
                Галерея вкусов – крути купол и выбирай, что приготовить
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {detailOpen &&
        createPortal(
          <Dialog open={detailOpen} onOpenChange={setDetailOpen}>
            <DialogContent className="max-w-md bg-white rounded-2xl" style={{ zIndex: 9999 }}>
              <DialogHeader>
                <DialogTitle className="text-2xl font-bold">Ваш выбор</DialogTitle>
              </DialogHeader>
              {selectedRecipe && (
                <div className="space-y-4">
                  <img
                    src={selectedRecipe.previewImageUrl}
                    alt={selectedRecipe.name}
                    className="w-full h-48 object-cover rounded-xl"
                  />
                  <p className="text-center text-lg font-medium">{selectedRecipe.name}</p>
                  <div className="flex justify-center gap-4 pt-2">
                    <Button variant="violet" onClick={handleGoToRecipe}>
                      Перейти к рецепту
                    </Button>
                    <Button variant="outline" onClick={() => setDetailOpen(false)}>
                      Закрыть
                    </Button>
                  </div>
                </div>
              )}
            </DialogContent>
          </Dialog>,
          document.body
        )}
    </>
  );
};

export default RecipeDomeModal;