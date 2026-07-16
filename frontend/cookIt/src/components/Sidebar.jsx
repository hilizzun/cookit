import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Button } from "./ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { 
  Home, 
  Clock, 
  BookOpen, 
  Heart, 
  Star, 
  LogOut,
  CirclePlus,
  Shield,
  ShoppingCart,        
} from "lucide-react";

const navItems = [
  { to: "/", label: "Главная", icon: Home },
  { to: "/recipes", label: "Все рецепты", icon: BookOpen },
  { to: "/recent-recipes", label: "Свежие рецепты", icon: Clock },
  { to: "/favorites", label: "Избранное", icon: Heart },
  { to: "/shopping-list", label: "Список покупок", icon: ShoppingCart },
  { to: "/top", label: "Топ", icon: Star },
  { to: "/recipes/create", label: "Создать рецепт", icon: CirclePlus },
];

export default function Sidebar() {
  const { user, userAvatar, logout } = useAuth();
  const location = useLocation();
  const isAdmin = user?.roles?.includes('Admin') || user?.roles?.includes('Moderator');

  const handleLogout = () => {
    logout();
  };

  return (
    <aside className="fixed top-4 left-4 bottom-4 w-64">
      <div className="h-full bg-white rounded-2xl shadow-sm flex flex-col overflow-hidden">
        <div className="p-6">
          <Link to="/">
            <img src="/logo.svg" alt="CookIt" className="h-13" />
          </Link>
        </div>

        <nav className="flex-1 px-4 space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;
            const isActive = location.pathname === item.to;
            return (
              <Button
                key={item.to}
                variant="ghost"
                className={`w-full justify-start gap-3 ${
                  isActive 
                    ? "bg-[#201469] text-white hover:bg-[#201469]/90" 
                    : "sidebar-nav-item"
                }`}
                asChild
              >
                <Link to={item.to}>
                  <Icon className="h-4 w-4" />
                  {item.label}
                </Link>
              </Button>
            );
          })}
          {isAdmin && (
            <Button
              variant="ghost"
              className={`w-full justify-start gap-3 ${
                location.pathname === '/admin' 
                  ? "bg-[#201469] text-white hover:bg-[#201469]/90" 
                  : "sidebar-nav-item"
              }`}
              asChild
            >
              <Link to="/admin">
                <Shield className="h-4 w-4" />
                Админ-панель
              </Link>
            </Button>
          )}
        </nav>

        <div className="p-4 border-t border-gray-100">
          {user ? (
            <div className="space-y-2">
              <Link 
                to="/profile" 
                className="flex items-center gap-3 p-2 rounded-xl sidebar-nav-item transition-colors"
              >
                <Avatar className="h-8 w-8">
                  {userAvatar ? (
                    <AvatarImage src={userAvatar} onError={(e) => e.target.style.display = 'none'} />
                  ) : null}
                  <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white">
                    {user.username?.charAt(0).toUpperCase()}
                  </AvatarFallback>
                </Avatar>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium truncate">{user.username}</p>
                  <p className="text-xs text-muted-foreground truncate">Профиль</p>
                </div>
              </Link>
              <Button 
                variant="ghost" 
                className="w-full justify-start gap-3 text-muted-foreground sidebar-nav-item"
                onClick={handleLogout}
              >
                <LogOut className="h-4 w-4" />
                Выйти
              </Button>
            </div>
          ) : (
            <div className="space-y-2">
              <Button variant="dashedHover" className="w-full" asChild>
                <Link to="/login">Войти</Link>
              </Button>
              <Button variant="default" className="w-full sidebar-nav-item" asChild>
                <Link to="/register">Регистрация</Link>
              </Button>
            </div>
          )}
        </div>
      </div>
    </aside>
  );
}