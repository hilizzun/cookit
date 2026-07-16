import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";

export default function BlockedGuard() {
  const { accessToken, isBlocked, isInitialized } = useAuth();

  if (!isInitialized) {
    return <div>Загрузка...</div>;
  }

  if (accessToken && isBlocked) {
    return <Navigate to="/blocked" replace />;
  }

  return <Outlet />;
}