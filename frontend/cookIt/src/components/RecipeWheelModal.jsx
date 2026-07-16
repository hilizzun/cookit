import React, { useState, useEffect, useRef } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from './ui/dialog';
import { Button } from './ui/button';
import { Loader2 } from 'lucide-react';
import { API_BASE_URL } from '../config/settings';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

export const RecipeWheelModal = ({ isOpen, onClose }) => {
  const { accessToken } = useAuth();
  const navigate = useNavigate();
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isSpinning, setIsSpinning] = useState(false);
  const [selectedRecipe, setSelectedRecipe] = useState(null);
  const [centerIndex, setCenterIndex] = useState(null);
  const wheelRef = useRef(null);
  const containerRef = useRef(null);
  const itemWidth = 336;
  const extraLoops = 2;
  const spinDuration = 3000;
  const repeats = 6;

  const wheelItems = React.useMemo(() => {
    if (!recipes.length) return [];
    return Array(repeats).fill(recipes).flat();
  }, [recipes]);

  useEffect(() => {
    if (isOpen) {
      fetchRandomRecipes();
    } else {
      setRecipes([]);
      setSelectedRecipe(null);
      setIsSpinning(false);
      setCenterIndex(null);
    }
  }, [isOpen]);

  const fetchRandomRecipes = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/random-wheel?count=12`, {
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

  const startSpin = () => {
    if (!recipes.length) return;
    setIsSpinning(true);
    fetch(`${API_BASE_URL}/api/wheel/spin`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${accessToken}`,
      },
    })
    setSelectedRecipe(null);
    setCenterIndex(null);

    const randomIdx = Math.floor(Math.random() * recipes.length);
    const targetRecipe = recipes[randomIdx];

    const containerWidth = containerRef.current?.offsetWidth || 800;
    const centerOffset = (containerWidth / 2) - (itemWidth / 2);
    const loopWidth = recipes.length * itemWidth;
    const targetIdx = randomIdx + recipes.length * 3;

    const targetX = -(targetIdx * itemWidth + extraLoops * loopWidth) + centerOffset;

    if (wheelRef.current) {
      wheelRef.current.style.transition = 'none';
      wheelRef.current.style.transform = 'translateX(0)';
      void wheelRef.current.offsetWidth;
      wheelRef.current.style.transition = `transform ${spinDuration}ms cubic-bezier(0.25, 1, 0.35, 1)`;
      wheelRef.current.style.transform = `translateX(${targetX}px)`;
    }

    setTimeout(() => {
      setIsSpinning(false);
      setSelectedRecipe(targetRecipe);
      const finalTransform = wheelRef.current?.style.transform;
      const match = finalTransform?.match(/-?\d+/);
      if (match) {
        const finalX = parseInt(match[0]);
        const centerIdx = Math.round(-finalX / itemWidth);
        setCenterIndex(centerIdx);
      }
    }, spinDuration);
  };

  const resetPosition = () => {
    if (wheelRef.current) {
      wheelRef.current.style.transition = 'none';
      wheelRef.current.style.transform = 'translateX(0)';
      void wheelRef.current.offsetWidth;
    }
  };

  const handleSpinAgain = () => {
    resetPosition();
    setTimeout(() => {
      startSpin();
    }, 10);
  };

  const handleGoToRecipe = () => {
    if (selectedRecipe) {
      onClose();
      navigate(`/recipes/${selectedRecipe.id}`);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-6xl bg-white min-h-[540px] flex flex-col overflow-hidden shadow-2xl">
        <DialogHeader className="pb-3">
          <DialogTitle className="text-3xl font-bold tracking-tight">
            Рецепт от Фортуны
          </DialogTitle>
          <p className="text-muted-foreground text-md">
            Что приготовить? Решит колесо!
          </p>
        </DialogHeader>

        {loading ? (
          <div className="flex-1 flex justify-center items-center">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        ) : (
          <div className="flex flex-col flex-1">
            <div className="relative mt-4 mb-6 h-96">
              <div className="absolute inset-y-0 -left-7 w-32 bg-gradient-to-r from-white to-transparent z-10 pointer-events-none" />
              <div className="absolute inset-y-0 -right-7 w-32 bg-gradient-to-l from-white to-transparent z-10 pointer-events-none" />

              <div className="absolute top-0 left-1/2 transform -translate-x-1/2 z-0 pointer-events-none h-full">
                <div className="w-1 h-full bg-yellow-500 shadow-md rounded-full" />
                <div className="absolute -top-3 left-1/2 transform -translate-x-1/2 w-5 h-5 bg-yellow-500 rounded-full shadow-sm" />
              </div>

              <div 
                ref={containerRef} 
                className="relative overflow-visible h-full mt-6"
              >
                <div
                  ref={wheelRef}
                  className="flex gap-4 h-full items-center"
                  style={{ transform: 'translateX(0)' }}
                >
              {wheelItems.map((recipe, index) => (
                <div
                  key={`${recipe.id}-${index}`}
                  className={`flex-shrink-0 w-80 p-2 transition-all duration-300 ${
                    !isSpinning && centerIndex === index - 1
                      ? 'scale-115 z-40'
                      : !isSpinning && selectedRecipe
                      ? 'opacity-50'
                      : ''
                  }`}
                >
                  <div className="relative w-full h-48 rounded-md overflow-hidden shadow-md">
                    <img
                      src={recipe.previewImageUrl}
                      alt={recipe.name}
                      className="w-full h-full object-cover"
                    />
                  </div>
                    <p className="pt-2 text-center font-medium truncate bg-white rounded-md py-1 px-2 w-full block">
                      {recipe.name}
                    </p>
                </div>
              ))}
                </div>
              </div>
            </div>

            <div className="min-h-[80px] flex items-center justify-center mt-2">
              {!isSpinning && !selectedRecipe && (
                <Button onClick={startSpin} variant="violet" size="lg" className="px-8 text-lg">
                  Крутить!
                </Button>
              )}

              {selectedRecipe && (
                <div className="text-center">
                  <div className="flex gap-3 justify-center">
                    <Button variant="violet" onClick={handleGoToRecipe} size="lg">
                      Перейти к рецепту
                    </Button>
                    <Button variant="outline" onClick={handleSpinAgain} size="lg">
                      Крутить ещё
                    </Button>
                  </div>
                </div>
              )}
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default RecipeWheelModal;