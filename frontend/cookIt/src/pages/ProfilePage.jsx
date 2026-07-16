import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { changePassword, deleteAccount } from "../services/authService";
import RecipeCard from '../components/RecipeCard';
import { FaCamera, FaStar, FaRegStar, FaClock, FaCheck, FaTimes, FaCog } from "react-icons/fa";
import { API_BASE_URL } from '../config/settings';
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Avatar, AvatarImage, AvatarFallback } from "../components/ui/avatar";
import { Badge } from "../components/ui/badge";
import { Card, CardContent } from "../components/ui/card";
import { Separator } from "../components/ui/separator";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "../components/ui/dialog";
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "../components/ui/tabs";
import { Loader2, Trophy } from "lucide-react";

export default function ProfilePage() {
  const { user, accessToken, userAvatar, uploadAvatar, deleteAvatar, logout } = useAuth();
  const navigate = useNavigate();
  const [userProfile, setUserProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [settingsOpen, setSettingsOpen] = useState(false);

  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: ""
  });
  const [deletePassword, setDeletePassword] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const [deleteAvatarDialogOpen, setDeleteAvatarDialogOpen] = useState(false);
  const [deleteAccountDialogOpen, setDeleteAccountDialogOpen] = useState(false);

  const fetchUserData = async () => {
    if (!accessToken || !user) return;
    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}/api/users/profile`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (response.ok) {
        const data = await response.json();
        setUserProfile(data);
      } else {
        throw new Error('Ошибка загрузки данных профиля');
      }
    } catch (error) {
      console.error("Ошибка загрузки данных пользователя:", error);
      setError("Не удалось загрузить данные профиля");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUserData();
  }, [accessToken, user]);

  const handleChangePassword = async (e) => {
    e.preventDefault();
    setError("");
    setMessage("");

    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setError("Новые пароли не совпадают");
      return;
    }
    if (passwordData.newPassword.length < 6) {
      setError("Пароль должен содержать минимум 6 символов");
      return;
    }

    setIsLoading(true);
    try {
      await changePassword(accessToken, {
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword
      });
      setMessage("Пароль успешно изменен");
      setPasswordData({
        currentPassword: "",
        newPassword: "",
        confirmPassword: ""
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteAccount = async () => {
    setError("");
    if (!deletePassword) {
      setError("Введите пароль для подтверждения");
      return;
    }
    setIsLoading(true);
    try {
      await deleteAccount(accessToken, deletePassword);
      await logout();
      navigate("/");
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
      setDeleteAccountDialogOpen(false);
    }
  };

  const handleDeleteAvatar = async () => {
    setIsLoading(true);
    try {
      await deleteAvatar();
      setMessage("Аватарка успешно удалена");
      setDeleteAvatarDialogOpen(false);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAvatarClick = () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/jpeg,image/png,image/gif,image/webp';
    input.onchange = async (e) => {
      const file = e.target.files[0];
      if (file) {
        if (file.size > 5 * 1024 * 1024) {
          setError('Размер файла не должен превышать 5MB');
          return;
        }
        setIsLoading(true);
        setError('');
        try {
          await uploadAvatar(file);
          setMessage('Аватарка успешно обновлена');
          await fetchUserData();
        } catch (err) {
          setError(err.message);
        } finally {
          setIsLoading(false);
        }
      }
    };
    input.click();
  };

  const handleToggleFavorite = async (recipeId, isFavorite) => {
    try {
      const url = `${API_BASE_URL}/api/favorites/${recipeId}`;
      const method = isFavorite ? 'DELETE' : 'POST';
      const response = await fetch(url, {
        method,
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка при обновлении избранного');
      setUserProfile(prevProfile => ({
        ...prevProfile,
        recipes: prevProfile.recipes.map(recipe =>
          recipe.id === recipeId ? { ...recipe, isFavorite: !isFavorite } : recipe
        )
      }));
    } catch (error) {
      console.error(error);
    }
  };

  const renderRecipeStatus = (recipe) => {
    if (recipe.isApproved === null) {
      return <Badge variant="outline" className="bg-yellow-100 text-yellow-800 border-yellow-300"><FaClock className="mr-1 h-3 w-3" />На проверке</Badge>;
    } else if (recipe.isApproved === false) {
      return <Badge variant="destructive"><FaTimes className="mr-1 h-3 w-3" />Отклонен</Badge>;
    } else if (recipe.isApproved === true) {
      return <Badge variant="default" className="bg-green-100 text-green-800 border-green-300"><FaCheck className="mr-1 h-3 w-3" />Утвержден</Badge>;
    }
    return null;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Профиль</h1>
        <p className="text-muted-foreground mt-2">Управление вашим аккаунтом и рецептами</p>
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

      <Card className="border-0 shadow-none bg-gray-50 rounded-xl">
        <CardContent className="p-0">
          <div className="flex items-start gap-6 p-6 bg-muted/30 rounded-xl">
            <div className="relative">
              <Avatar className="h-24 w-24">
                {userAvatar ? (
                  <AvatarImage src={userAvatar} onError={(e) => e.target.style.display = 'none'} />
                ) : (
                  <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white text-2xl">
                    {userProfile?.username?.charAt(0).toUpperCase() || 'U'}
                  </AvatarFallback>
                )}
              </Avatar>
              <Button
                variant="outline"
                size="icon"
                className="absolute -bottom-2 -right-2 rounded-full bg-background"
                onClick={handleAvatarClick}
                disabled={isLoading}
              >
                <FaCamera className="h-4 w-4" />
              </Button>
            </div>
            <div className="flex-1">
              <h2 className="text-2xl font-bold">{user?.username}</h2>
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
              </div>
            </div>
            <Dialog open={settingsOpen} onOpenChange={setSettingsOpen}>
              <div className="flex gap-2">
                <DialogTrigger asChild>
                  <Button variant="outline" size="icon">
                    <FaCog className="h-5 w-5" />
                  </Button>
                </DialogTrigger>
                <Button variant="outline" asChild className="gap-2">
                  <Link to="/achievements">
                    <Trophy className="h-4 w-4" /> Достижения
                  </Link>
                </Button>
              </div>
              <DialogContent className="sm:max-w-md bg-white">
                <DialogHeader>
                  <DialogTitle>Настройки профиля</DialogTitle>
                </DialogHeader>
                <Tabs defaultValue="password" className="mt-4">
                  <TabsList className="grid w-full grid-cols-3">
                    <TabsTrigger value="password">Пароль</TabsTrigger>
                    <TabsTrigger value="avatar">Аватар</TabsTrigger>
                    <TabsTrigger value="delete">Удаление</TabsTrigger>
                  </TabsList>
                  <TabsContent value="password" className="space-y-4 mt-4">
                    <form onSubmit={handleChangePassword} className="space-y-4">
                      <div className="space-y-2">
                        <Label htmlFor="currentPassword">Текущий пароль</Label>
                        <Input
                          id="currentPassword"
                          type="password"
                          value={passwordData.currentPassword}
                          onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="newPassword">Новый пароль</Label>
                        <Input
                          id="newPassword"
                          type="password"
                          value={passwordData.newPassword}
                          onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                          required
                        />
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="confirmPassword">Подтвердите новый пароль</Label>
                        <Input
                          id="confirmPassword"
                          type="password"
                          value={passwordData.confirmPassword}
                          onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                          required
                        />
                      </div>
                      <Button type="submit" variant="violet" className="w-full" disabled={isLoading}>
                        {isLoading ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
                        Сменить пароль
                      </Button>
                    </form>
                  </TabsContent>
                  <TabsContent value="avatar" className="space-y-4 mt-4">
                    <div className="flex items-center gap-4">
                      <Avatar className="h-24 w-24">
                        {userAvatar ? (
                          <AvatarImage src={userAvatar} onError={(e) => e.target.style.display = 'none'} />
                        ) : (
                          <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white text-2xl">
                            {userProfile?.username?.charAt(0).toUpperCase() || 'U'}
                          </AvatarFallback>
                        )}
                      </Avatar>
                      <div className="space-y-2">
                        <Button variant="outline" size="sm" onClick={handleAvatarClick} disabled={isLoading}>
                          <FaCamera className="mr-2 h-4 w-4" />
                          Загрузить новую
                        </Button>
                        {userAvatar && (
                          <Button
                            variant="destructive"
                            size="sm"
                            onClick={() => setDeleteAvatarDialogOpen(true)}
                            disabled={isLoading}
                          >
                            Удалить
                          </Button>
                        )}
                      </div>
                    </div>
                  </TabsContent>
                  <TabsContent value="delete" className="space-y-4 mt-4">
                    <div className="rounded-lg bg-destructive/10 p-4 border border-destructive/20">
                      <h4 className="font-medium text-destructive mb-2">Удаление аккаунта</h4>
                      <p className="text-sm text-muted-foreground mb-4">
                        Это действие необратимо. Все ваши данные будут удалены.
                      </p>
                      <div className="space-y-2">
                        <Label htmlFor="deletePassword">Введите пароль для подтверждения</Label>
                        <Input
                          id="deletePassword"
                          type="password"
                          value={deletePassword}
                          onChange={(e) => setDeletePassword(e.target.value)}
                        />
                      </div>
                      <Button
                        variant="destructive"
                        className="w-full mt-4"
                        onClick={() => setDeleteAccountDialogOpen(true)}
                        disabled={!deletePassword}
                      >
                        Удалить аккаунт
                      </Button>
                    </div>
                  </TabsContent>
                </Tabs>
              </DialogContent>
            </Dialog>
          </div>
        </CardContent>
      </Card>

      <Separator />

      <div>
        <h2 className="text-2xl font-semibold mb-4">Мои рецепты</h2>
        {userProfile?.recipes && userProfile.recipes.length > 0 ? (
          <>
            <div className="flex gap-4 text-sm text-muted-foreground mb-4">
              <span>Всего: {userProfile.recipes.length}</span>
              {userProfile.recipes.filter(r => r.isApproved === null).length > 0 && (
                <span className="text-yellow-600">На проверке: {userProfile.recipes.filter(r => r.isApproved === null).length}</span>
              )}
              {userProfile.recipes.filter(r => r.isApproved === false).length > 0 && (
                <span className="text-destructive">Отклонено: {userProfile.recipes.filter(r => r.isApproved === false).length}</span>
              )}
            </div>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {userProfile.recipes.map((recipe) => (
                <div key={recipe.id} className="relative">
                  <RecipeCard
                    recipe={recipe}
                    onToggleFavorite={handleToggleFavorite}
                  />
                  <div className="absolute bottom-18 left-2">
                    {renderRecipeStatus(recipe)}
                  </div>
                </div>
              ))}
            </div>
          </>
        ) : (
          <Card className="border-0 shadow-none bg-muted/30">
            <CardContent className="flex flex-col items-center py-12">
              <span className="text-6xl mb-4">🍳</span>
              <h3 className="text-xl font-semibold mb-2">Рецептов пока нет</h3>
              <p className="text-muted-foreground mb-4">Создайте свой первый рецепт!</p>
              <Button variant="violet" asChild>
                <Link to="/recipes/create">Создать рецепт</Link>
              </Button>
            </CardContent>
          </Card>
        )}
      </div>

      <AlertDialog open={deleteAvatarDialogOpen} onOpenChange={setDeleteAvatarDialogOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Подтверждение удаления</AlertDialogTitle>
            <AlertDialogDescription>
              Вы уверены, что хотите удалить аватарку?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeleteAvatarDialogOpen(false)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteAvatar} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Удалить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={deleteAccountDialogOpen} onOpenChange={setDeleteAccountDialogOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Подтверждение удаления аккаунта</AlertDialogTitle>
            <AlertDialogDescription>
              Это действие невозможно отменить. Все ваши данные будут безвозвратно удалены. Вы уверены?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeleteAccountDialogOpen(false)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteAccount} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Удалить аккаунт
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}