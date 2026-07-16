import { API_BASE_URL } from '../config/settings';

export const getMyAchievements = async (accessToken) => {
  const response = await fetch(`${API_BASE_URL}/api/statistics/me`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!response.ok) throw new Error('Ошибка загрузки достижений');
  return response.json();
};

export const getUserAchievements = async (accessToken, userId) => {
  const response = await fetch(`${API_BASE_URL}/api/statistics/user/${userId}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
  if (!response.ok) throw new Error('Ошибка загрузки достижений пользователя');
  return response.json();
};