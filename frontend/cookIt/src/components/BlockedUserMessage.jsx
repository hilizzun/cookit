import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Button } from './ui/button';
import { Card, CardContent } from './ui/card';
import { Ban, LogOut } from 'lucide-react';

const BlockedUserMessage = () => {
  const { user, logout, isBlocked, accessToken } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!accessToken) {
      navigate('/login');
      return;
    }
    if (accessToken && !isBlocked && !user?.isBlocked) {
      navigate('/');
      return;
    }
  }, [accessToken, isBlocked, user?.isBlocked, navigate]);

  if (accessToken && (isBlocked || user?.isBlocked)) {
    const blockedUntil = user?.blockedUntil ? new Date(user.blockedUntil).toLocaleDateString('ru-RU') : null;

    return (
      <div className="flex items-center justify-center h-full p-4">
        <Card className="max-w-md w-full border-0 shadow-none">
          <CardContent className="flex flex-col items-center p-8 text-center">
            <Ban className="h-16 w-16 text-destructive mb-4" />
            <h1 className="text-2xl font-bold mb-2">Ваш аккаунт заблокирован</h1>
            <p className="text-muted-foreground mb-6">
              Вы не можете использовать сайт, пока ваш аккаунт заблокирован.
            </p>
            <div className="w-full bg-destructive/10 p-4 rounded-lg text-left mb-6">
              <p className="font-semibold text-destructive mb-1">Причина блокировки:</p>
              <p className="text-destructive/80 text-sm mb-2">{user?.blockedReason || "Не указана"}</p>
              {blockedUntil ? (
                <p className="text-destructive/80 text-sm">Блокировка до: {blockedUntil}</p>
              ) : (
                <p className="text-destructive/80 text-sm">Блокировка бессрочная</p>
              )}
            </div>
            <Button variant="destructive" className="w-full" onClick={logout}>
              <LogOut className="mr-2 h-4 w-4" /> Выйти из аккаунта
            </Button>
            <p className="text-sm text-muted-foreground mt-4">
              Если вы считаете, что блокировка произошла по ошибке, свяжитесь с администрацией.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return null;
};

export default BlockedUserMessage;