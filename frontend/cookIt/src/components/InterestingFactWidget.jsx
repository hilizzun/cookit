import React, { useEffect, useState, useCallback } from 'react';
import { X, RefreshCw } from 'lucide-react';
import { Button } from './ui/button';
import { Card } from './ui/card';
import { API_BASE_URL } from '../config/settings';

const InterestingFactWidget = ({ recipeId }) => {
  const [facts, setFacts] = useState([]);
  const [currentFact, setCurrentFact] = useState(null);
  const [isVisible, setIsVisible] = useState(true);
  let timeoutId = null;

  useEffect(() => {
    const fetchFacts = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/facts/recipe/${recipeId}`);
        const data = await response.json();
        const factsArray = Array.isArray(data) ? data : (data.$values || []);
        setFacts(factsArray);
        if (factsArray.length > 0) {
          const randomIndex = Math.floor(Math.random() * factsArray.length);
          setCurrentFact(factsArray[randomIndex]);
        } else {
          setIsVisible(false);
        }
      } catch (error) {
        console.error('Ошибка загрузки фактов:', error);
        setIsVisible(false);
      }
    };
    fetchFacts();
  }, [recipeId]);

  useEffect(() => {
    if (currentFact && isVisible) {
      timeoutId = setTimeout(() => {
        setIsVisible(false);
      }, 15000);
    }
    return () => {
      if (timeoutId) clearTimeout(timeoutId);
    };
  }, [currentFact, isVisible]);

  const handleNextFact = () => {
    if (facts.length === 0) return;
    let newIndex;
    do {
      newIndex = Math.floor(Math.random() * facts.length);
    } while (facts.length > 1 && facts[newIndex] === currentFact);
    setCurrentFact(facts[newIndex]);
    if (timeoutId) clearTimeout(timeoutId);
    timeoutId = setTimeout(() => {
      setIsVisible(false);
    }, 15000);
  };

  if (!isVisible || !currentFact) return null;

  return (
    <div className="fixed top-15 right-20 z-50 w-80">
      <Card className="p-3 bg-white shadow-lg border border-violet-200 rounded-lg">
        <div className="flex justify-between items-start gap-2">
          <div className="flex-1">
            <div className="flex items-center gap-1 mb-1">
              <span className="text-xs font-semibold text-violet-900 uppercase tracking-wide">
                Интересный факт
              </span>
            </div>
            <p className="text-sm text-gray-700">{currentFact}</p>
          </div>
          <Button
            variant="ghost"
            size="icon"
            className="h-5 w-5 text-gray-400 hover:text-gray-600 -mt-1 -mr-1"
            onClick={() => setIsVisible(false)}
          >
            <X className="h-3 w-3" />
          </Button>
        </div>
        <div className="mt-2 flex justify-end">
          <Button
            variant="ghost"
            size="sm"
            onClick={handleNextFact}
            className="text-xs text-violet-700 hover:text-violet-900 gap-1 h-7 px-2"
          >
            <RefreshCw className="h-3 w-3" />
            Другой факт
          </Button>
        </div>
      </Card>
    </div>
  );
};

export default InterestingFactWidget;