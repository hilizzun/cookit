import React, { useEffect, useState } from 'react';
import * as Tooltip from '@radix-ui/react-tooltip';
import { Loader2 } from 'lucide-react';
import { API_BASE_URL } from '../config/settings';
import { useAuth } from '../contexts/AuthContext';

const achievementMeta = {
  RecipesPublished: { title: 'Публикатор', prefix: 'recipes' },
  FavoritesAdded: { title: 'Коллекционер', prefix: 'favorites' },
  ShoppingListAdded: { title: 'Покупатель', prefix: 'shopping' },
  CommentsLeft: { title: 'Комментатор', prefix: 'comments' },
  WheelSpins: { title: 'Любитель фортуны', prefix: 'wheel' },
  FiveStarRatingsReceived: { title: 'Народный любимец', prefix: 'stars' },
};

const UserAchievementsTooltip = ({ userId, children }) => {
  const { accessToken } = useAuth();
  const [achievements, setAchievements] = useState(null);
  const [loading, setLoading] = useState(false);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    if (!open || !userId || achievements !== null) return;
    const fetchAchievements = async () => {
      setLoading(true);
      try {
        const res = await fetch(`${API_BASE_URL}/api/statistics/user/${userId}`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (res.ok) {
          const data = await res.json();
          setAchievements(data.achievements || []);
        } else {
          console.error('Failed to fetch achievements');
        }
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchAchievements();
  }, [open, userId, accessToken, achievements]);

  return (
    <Tooltip.Provider>
      <Tooltip.Root open={open} onOpenChange={setOpen} delayDuration={300}>
        <Tooltip.Trigger asChild>{children}</Tooltip.Trigger>
        <Tooltip.Portal>
          <Tooltip.Content
            className="z-50 min-w-[330px] max-w-sm bg-white rounded-xl shadow-xl p-4 border border-gray-200"
            sideOffset={5}
          >
            {loading && (
              <div className="flex justify-center py-4">
                <Loader2 className="h-5 w-5 animate-spin text-muted-foreground" />
              </div>
            )}
            <div className="text-sm font-semibold border-b border-gray-100 pb-2 mb-2">
            Достижения пользователя
            </div>
            {!loading && achievements && achievements.length > 0 && (
              <div className="space-y-3">
                {achievements.map(ach => {
                  const meta = achievementMeta[ach.type];
                  if (!meta) return null;
                  const isUnlocked = ach.level > 0;
                  const iconUrl = isUnlocked
                    ? `${API_BASE_URL}/achievements/${meta.prefix}_${ach.level}.png`
                    : `${API_BASE_URL}/achievements/${meta.prefix}_locked_1.png`;
                  return (
                    <div key={ach.type} className="flex items-center gap-3">
                      <img
                        src={iconUrl}
                        alt={meta.title}
                        className="w-14 h-14 object-contain flex-shrink-0"
                        onError={(e) => (e.target.style.display = 'none')}
                      />
                      <div className="flex-1">
                        <div className="flex justify-between items-baseline">
                          <span className="font-medium text-sm">{meta.title}</span>
                          <span className="text-xs text-muted-foreground">
                            Уровень {ach.level}
                          </span>
                        </div>
                        <div className="text-xs text-muted-foreground mt-0.5">
                          {ach.type === 'RecipesPublished' ? 'рецептов: ' :
                            ach.type === 'FavoritesAdded' ? 'в избранном: ' :
                            ach.type === 'ShoppingListAdded' ? 'в списке покупок: ' :
                            ach.type === 'CommentsLeft' ? 'комментариев: ' :
                            ach.type === 'WheelSpins' ? 'вращений: ' :
                            ach.type === 'FiveStarRatingsReceived' ? 'оценок "5": ' : ''} {ach.currentValue} 
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
            {!loading && (!achievements || achievements.length === 0) && (
              <div className="text-sm text-muted-foreground">Нет достижений</div>
            )}
            <Tooltip.Arrow className="fill-white" />
          </Tooltip.Content>
        </Tooltip.Portal>
      </Tooltip.Root>
    </Tooltip.Provider>
  );
};

export default UserAchievementsTooltip;