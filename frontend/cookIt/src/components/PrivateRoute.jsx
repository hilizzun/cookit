import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

export default function PrivateRoute() {
  const { accessToken, isInitialized } = useAuth();

  if (!isInitialized) {
    return <div>Загрузка...</div>;
  }

  return accessToken ? <Outlet /> : <Navigate to="/login" replace />;
}