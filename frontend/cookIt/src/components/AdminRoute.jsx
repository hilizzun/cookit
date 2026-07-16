import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const AdminRoute = () => {
  const { user } = useAuth();
  const isAdmin = user?.roles?.includes('Admin') || user?.roles?.includes('Moderator');
  
  return isAdmin ? <Outlet /> : <Navigate to="/" replace />;
};

export default AdminRoute;