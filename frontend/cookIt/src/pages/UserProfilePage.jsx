import React, { useState, useEffect } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import RecipeCard from '../components/RecipeCard';
import { API_BASE_URL } from '../config/settings';
import { Button } from "../components/ui/button";
import { Avatar, AvatarImage, AvatarFallback } from "../components/ui/avatar";
import { Card, CardContent } from "../components/ui/card";
import { Separator } from "../components/ui/separator";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "../components/ui/alert-dialog";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "../components/ui/dialog";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Textarea } from "../components/ui/textarea";
import { Loader2, ArrowLeft, Ban, CheckCircle, AlertTriangle } from "lucide-react";
import { FaStar, FaRegStar } from "react-icons/fa";

export default function UserProfilePage() {
  const { userId } = useParams();
  const { accessToken, user } = useAuth();
  const navigate = useNavigate();
  const [userProfile, setUserProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [isBlockDialogOpen, setIsBlockDialogOpen] = useState(false);
  const [isUnblockDialogOpen, setIsUnblockDialogOpen] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [message, setMessage] = useState("");
  const [blockReason, setBlockReason] = useState("");
  const [blockUntil, setBlockUntil] = useState("");
  const [blockError, setBlockError] = useState("");

  useEffect(() => {
    const fetchUserData = async () => {
      try {
        setLoading(true);
        setError("");
        const response = await fetch(`${API_BASE_URL}/api/users/${userId}/public-profile`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (response.ok) {
          const data = await response.json();
          setUserProfile(data);
        } else if (response.status === 404) {
          setError("Пользователь не найден");
        } else {
          throw new Error('Ошибка загрузки профиля пользователя');
        }
      } catch (error) {
        console.error("Ошибка загрузки данных пользователя:", error);
        setError("Не удалось загрузить профиль пользователя");
      } finally {
        setLoading(false);
      }
    };
    if (userId && accessToken) {
      fetchUserData();
    }
  }, [userId, accessToken]);

  const handleBlockUser = async () => {
    if (!blockReason.trim()) {
      setBlockError("Укажите причину блокировки");
      return;
    }
    setIsProcessing(true);
    try {
      const blockUntilDate = blockUntil ? new Date(blockUntil) : null;
      const response = await fetch(`${API_BASE_URL}/api/users/admin/block`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({
          userId: userId,
          reason: blockReason.trim(),
          blockUntil: blockUntilDate,
        }),
      });
      if (!response.ok) {
        const err = await response.json();
        throw new Error(err.message || 'Ошибка при блокировке пользователя');
      }
      // Обновляем данные
      const userResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/public-profile`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (userResponse.ok) {
        const data = await userResponse.json();
        setUserProfile(data);
        setMessage("Пользователь успешно заблокирован");
      }
      setIsBlockDialogOpen(false);
      setBlockReason("");
      setBlockUntil("");
      setBlockError("");
    } catch (error) {
      setError(error.message);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleUnblockUser = async () => {
    setIsProcessing(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/users/admin/unblock/${userId}`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) {
        const err = await response.json();
        throw new Error(err.message || 'Ошибка при разблокировке пользователя');
      }
      const userResponse = await fetch(`${API_BASE_URL}/api/users/${userId}/public-profile`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (userResponse.ok) {
        const data = await userResponse.json();
        setUserProfile(data);
        setMessage("Пользователь успешно разблокирован");
      }
      setIsUnblockDialogOpen(false);
    } catch (error) {
      setError(error.message);
    } finally {
      setIsProcessing(false);
    }
  };

  const handleBack = () => navigate(-1);

  const isOwnProfile = user?.userId === userId;
  const isAdmin = user?.roles?.includes("Admin");
  const canBlockUser = isAdmin && !isOwnProfile;

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error && !userProfile) {
    return (
      <div className="flex items-center justify-center h-full">
        <Card className="border-0 shadow-none bg-transparent">
          <CardContent className="flex flex-col items-center py-12">
            <span className="text-6xl mb-4">👤</span>
            <h2 className="text-2xl font-bold mb-2">Пользователь не найден</h2>
            <p className="text-muted-foreground mb-6">Запрошенный профиль не существует</p>
            <Button onClick={handleBack} variant="violet">
              <ArrowLeft className="mr-2 h-4 w-4" /> Назад
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <Button variant="ghost" size="sm" onClick={handleBack} className="mb-4">
        <ArrowLeft className="mr-2 h-4 w-4" /> Назад
      </Button>

      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          {isOwnProfile ? "Мой профиль" : "Профиль пользователя"}
        </h1>
        <p className="text-muted-foreground mt-2">
          {isOwnProfile ? "Ваша публичная страница" : "Публичная страница пользователя"}
        </p>
      </div>

      {message && (
        <div className="p-4 bg-green-50 text-green-800 rounded-lg border border-green-200">
          {message}
        </div>
      )}
      {error && (
        <div className="p-4 bg-destructive/10 text-destructive rounded-lg border border-destructive/20">
          {error}
        </div>
      )}

      {userProfile?.isBlocked && (
        <Card className="border-0 shadow-none bg-red-50">
          <CardContent className="p-4">
            <div className="flex items-start gap-3">
              <AlertTriangle className="text-destructive h-5 w-5 mt-0.5" />
              <div>
                <h3 className="font-semibold text-destructive mb-1">Пользователь заблокирован</h3>
                {userProfile.blockedReason && (
                  <p className="text-destructive/80 text-sm mb-1">Причина: {userProfile.blockedReason}</p>
                )}
                {userProfile.blockedUntil && (
                  <p className="text-destructive/80 text-sm">
                    Блокировка до: {new Date(userProfile.blockedUntil).toLocaleDateString('ru-RU')}
                  </p>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Card className="border-0 shadow-none bg-muted/30 rounded-xl bg-gray-50">
        <CardContent className="p-6">
          <div className="flex items-start gap-6">
          <Avatar className="h-24 w-24">
            {userProfile?.avatarUrl ? (
              <AvatarImage src={userProfile.avatarUrl} onError={(e) => e.target.style.display = 'none'} />
            ) : (
              <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white text-2xl">
                  {userProfile?.username?.charAt(0).toUpperCase() || 'U'}
                </AvatarFallback>
            )}
          </Avatar>
            <div className="flex-1">
              <h2 className="text-2xl font-bold">{userProfile?.username}</h2>
              <div className="flex items-center gap-4 mt-2">
                <div className="flex items-center gap-1">
                  {[...Array(5)].map((_, i) => (
                    <span key={i}>
                      {i < Math.floor(userProfile?.summary?.averageRating || 0) ? (
                        <FaStar className="text-yellow-500 h-5 w-5" />
                      ) : (
                        <FaRegStar className="text-muted-foreground h-5 w-5" />
                      )}
                    </span>
                  ))}
                  <span className="ml-2 font-semibold">
                    {(userProfile?.summary?.averageRating || 0).toFixed(1)}
                  </span>
                </div>
                <span className="text-muted-foreground">
                  Рецептов: {userProfile?.recipes?.length || 0}
                </span>
                {userProfile?.summary?.totalRatings > 0 && (
                  <span className="text-muted-foreground">
                    Оценок: {userProfile.summary.totalRatings}
                  </span>
                )}
                {canBlockUser && (
                  <div className="flex gap-3">
                    {!userProfile?.isBlocked ? (
                      <Button variant="dashedHover" onClick={() => setIsBlockDialogOpen(true)}>
                        <Ban className="mr-2 h-4 w-4" /> Заблокировать
                      </Button>
                    ) : (
                      <Button variant="default" className="bg-green-100 hover:bg-green-700" onClick={() => setIsUnblockDialogOpen(true)}>
                        <CheckCircle className="mr-2 h-4 w-4" /> Разблокировать
                      </Button>
                    )}
                  </div>
                )}
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <Separator />

      <div>
        <h2 className="text-2xl font-semibold mb-4">
          {isOwnProfile ? "Мои рецепты" : "Рецепты пользователя"}
        </h2>
        {userProfile?.recipes && userProfile.recipes.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {userProfile.recipes.map((recipe) => (
              <RecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>
        ) : (
          <Card className="border-0 shadow-none bg-muted/30">
            <CardContent className="flex flex-col items-center py-12">
              <span className="text-6xl mb-4">🍳</span>
              <h3 className="text-xl font-semibold mb-2">
                {isOwnProfile ? "У вас пока нет рецептов" : "У пользователя пока нет рецептов"}
              </h3>
            </CardContent>
          </Card>
        )}
      </div>

      <Dialog open={isBlockDialogOpen} onOpenChange={setIsBlockDialogOpen}>
        <DialogContent className="sm:max-w-md bg-white">
          <DialogHeader>
            <DialogTitle>Блокировка пользователя</DialogTitle>
            <DialogDescription>
              Вы собираетесь заблокировать пользователя <strong>{userProfile?.username}</strong>.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-2">
            {blockError && (
              <div className="p-2 bg-destructive/10 text-destructive rounded text-sm">
                {blockError}
              </div>
            )}
            <div className="space-y-2">
              <Label htmlFor="reason">Причина блокировки *</Label>
              <Textarea
                id="reason"
                value={blockReason}
                onChange={(e) => {
                  setBlockReason(e.target.value);
                  if (blockError) setBlockError("");
                }}
                placeholder="Укажите причину..."
                rows={3}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="until">Дата окончания блокировки (необязательно)</Label>
              <Input
                id="until"
                type="date"
                value={blockUntil}
                onChange={(e) => setBlockUntil(e.target.value)}
                min={new Date().toISOString().split('T')[0]}
              />
              <p className="text-sm text-muted-foreground">
                Если не указать дату, блокировка будет бессрочной
              </p>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsBlockDialogOpen(false)}>
              Отмена
            </Button>
            <Button variant="destructive" onClick={handleBlockUser} disabled={isProcessing}>
              {isProcessing && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Заблокировать
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <AlertDialog open={isUnblockDialogOpen} onOpenChange={setIsUnblockDialogOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Разблокировка пользователя</AlertDialogTitle>
            <AlertDialogDescription>
              Вы уверены, что хотите разблокировать пользователя {userProfile?.username}?
              Это действие восстановит его возможность использовать сайт.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setIsUnblockDialogOpen(false)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={handleUnblockUser} className="bg-green-600 text-white hover:bg-green-700">
              {isProcessing && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Разблокировать
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}