import React, { useEffect, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getMyAchievements } from '../services/achievementService';
import { Loader2 } from 'lucide-react';
import { Card, CardContent } from '../components/ui/card';
import { API_BASE_URL } from '../config/settings';

const PROGRESS_THRESHOLDS = [1, 10, 50, 100];

const LEVELS = [1, 2, 3, 4];

const achievementMeta = {
  RecipesPublished: {
    title: 'Публикатор',
    description: 'Опубликуйте рецепты, чтобы делиться кулинарными идеями.',
  },
  FavoritesAdded: {
    title: 'Коллекционер',
    description: 'Добавляйте рецепты в избранное, чтобы не потерять.',
  },
  ShoppingListAdded: {
    title: 'Покупатель',
    description: 'Добавляйте рецепты в список покупок для удобного похода в магазин.',
  },
  CommentsLeft: {
    title: 'Комментатор',
    description: 'Оставляйте комментарии, делитесь мнением о рецептах.',
  },
  WheelSpins: {
    title: 'Любитель фортуны',
    description: 'Крутите колесо и открывайте случайные рецепты.',
  },
  FiveStarRatingsReceived: {
    title: 'Народный любимец',
    description: 'Получайте оценки 5 звезд от других пользователей.',
  },
};

const typeToFilePrefix = {
  RecipesPublished: 'recipes',
  FavoritesAdded: 'favorites',
  ShoppingListAdded: 'shopping',
  CommentsLeft: 'comments',
  WheelSpins: 'wheel',
  FiveStarRatingsReceived: 'stars',
};

const AchievementsPage = () => {
  const { accessToken } = useAuth();
  const [achievements, setAchievements] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchAchievements = async () => {
      try {
        const data = await getMyAchievements(accessToken);
        setAchievements(data.achievements || []);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    fetchAchievements();
  }, [accessToken]);

  if (loading) {
    return (
      <div className="flex justify-center py-12">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="space-y-8 max-w-4xl mx-auto">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Мои достижения</h1>
        <p className="text-muted-foreground mt-2">
          Открывайте новые уровни, становясь активнее на сайте
        </p>
      </div>

      <div className="space-y-6">
        {achievements.map(ach => {
          const meta = achievementMeta[ach.type] || { title: ach.type, description: '' };
          let nextThreshold = null;
          for (const t of PROGRESS_THRESHOLDS) {
            if (ach.currentValue < t) {
              nextThreshold = t;
              break;
            }
          }
          const progressPercent = nextThreshold ? (ach.currentValue / nextThreshold) * 100 : 100;

          return (
            <Card key={ach.type} className="overflow-hidden border shadow-sm">
            <CardContent className="p-5">
              <div className="flex flex-col gap-4">
                <div className="flex-1">
                  <h3 className="text-xl font-semibold">{meta.title}</h3>
                  <p className="text-sm text-muted-foreground mt-1">{meta.description}</p>
                  <div className="mt-2 flex items-center gap-2 text-sm">
                    <span className="font-medium">Прогресс:</span>
                    <span>{ach.currentValue} / {nextThreshold ?? 'Max'}</span>
                  </div>
                  <div className="mt-2 h-2 bg-gray-200 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-violet-600 rounded-full transition-all duration-300"
                      style={{ width: `${Math.min(progressPercent, 100)}%` }}
                    />
                  </div>
                </div>

                <div className="flex gap-4 items-center justify-center mt-4">
                  {LEVELS.map(level => {
                    const isUnlocked = ach.level >= level;
                    const prefix = typeToFilePrefix[ach.type];
                    const iconPath = isUnlocked
                      ? `${API_BASE_URL}/achievements/${prefix}_${level}.png`
                      : `${API_BASE_URL}/achievements/${prefix}_locked_${level}.png`;
                    return (
                      <div key={level} className="text-center w-38">
                        <img
                          src={iconPath}
                          alt={`Уровень ${level}`}
                          className={`w-38 h-38 mx-auto object-contain transition-opacity ${!isUnlocked ? 'opacity-50 grayscale' : ''}`}
                          onError={(e) => (e.target.style.display = 'none')}
                        />
                      </div>
                    );
                  })}
                </div>
              </div>
            </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
  );
};

export default AchievementsPage;