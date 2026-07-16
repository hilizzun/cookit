import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import RecipesPage from './pages/RecipesPage';
import PrivateRoute from './components/PrivateRoute';
import BlockedGuard from './components/BlockedGuard';
import RecipeDetailPage from './pages/RecipeDetailPage';
import RecipeEditPage from './pages/RecipeEditPage'; 
import RecentRecipesPage from './pages/RecentRecipesPage';
import TopRecipesPage from './pages/TopRecipesPage';
import RecipeCreatePage from './pages/RecipeCreatePage';
import ConfirmEmailPage from './pages/ConfirmEmailPage'
import EmailConfirmedPage from './pages/EmailConfirmedPage';
import ResendVerificationPage from './pages/ResendVerificationPage';
import EmailVerificationSentPage from './pages/EmailVerificationSentPage'
import FavoritesPage from './pages/FavoritesPage'
import ProfilePage from './pages/ProfilePage';
import UserProfilePage from './pages/UserProfilePage';
import BlockedUserMessage from './components/BlockedUserMessage'; 
import Layout from './components/Layout';
import AdminPage from './pages/AdminPage';
import AdminRoute from './components/AdminRoute';
import ShoppingListPage from './pages/ShoppingListPage';
import AchievementsPage from './pages/AchievementsPage';
import { ShoppingListProvider } from './contexts/ShoppingListContext';

function App() {
  return (
    <ShoppingListProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/blocked" element={<BlockedUserMessage />} />
        
        <Route element={<Layout />}>
          {/* Все остальные маршруты обернуты в BlockedGuard */}
          <Route element={<BlockedGuard />}>
            <Route path="/" element={<HomePage />} />
            <Route path="/confirm-email/:userId/:token" element={<ConfirmEmailPage />} />
            <Route path="/email-confirmed" element={<EmailConfirmedPage />} />
            <Route path="/resend-verification" element={<ResendVerificationPage />} />
            <Route path="/email-verification-sent" element={<EmailVerificationSentPage />} />
            
            {/* Маршруты, требующие авторизации, обернуты в PrivateRoute */}
            <Route element={<PrivateRoute />}>
              <Route path="/recipes/create" element={<RecipeCreatePage />} />
              <Route path="/recipes" element={<RecipesPage />} />
              <Route path="/recipes/:id" element={<RecipeDetailPage />} />
              <Route path="/recipes/edit/:id" element={<RecipeEditPage />} />
              <Route path="/recent-recipes" element={<RecentRecipesPage />} />
              <Route path="/favorites" element={<FavoritesPage />} />
              <Route path="/top" element={<TopRecipesPage />} />
              <Route path="/profile" element={<ProfilePage />} />
              <Route path="/users/:userId" element={<UserProfilePage />} />
              <Route path="/shopping-list" element={<ShoppingListPage />} />
              <Route path="/achievements" element={<AchievementsPage />} />
              <Route element={<AdminRoute />}>
                <Route path="/admin" element={<AdminPage />} />
              </Route>
            </Route>
          </Route>
        </Route>
      </Routes>
    </ShoppingListProvider>  
  );
}

export default App;