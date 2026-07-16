import React, { createContext, useContext, useState, useEffect } from "react";
import { 
  refreshToken, 
  logoutUser, 
  loginUser, 
  getAvatarUrl, 
  uploadAvatar as uploadAvatarService, 
  deleteAvatar as deleteAvatarService 
} from "../services/authService";
import { useNavigate } from "react-router-dom";

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const navigate = useNavigate();
  const [user, setUser] = useState(null);
  const [accessToken, setAccessToken] = useState(null);
  const [isInitialized, setIsInitialized] = useState(false);
  const [userAvatar, setUserAvatar] = useState(null);
  const [avatarLoading, setAvatarLoading] = useState(false);
  const [isBlocked, setIsBlocked] = useState(false);

  const login = async (credentials) => {
    try {
      const data = await loginUser(credentials);
      
      console.log("AuthContext login data:", data);
      
      // Проверяем, заблокирован ли пользователь
      if (data.isBlocked) {
        setUser({ 
          username: data.userName || data.username, 
          userId: data.userId || data.userID, 
          roles: data.roles || [],
          email: data.email || credentials.email,
          isBlocked: true,
          blockedReason: data.blockedReason,
          blockedUntil: data.blockedUntil
        });
        setIsBlocked(true);
        localStorage.setItem("accessToken", data.accessToken || data.access_token);
        navigate('/blocked');
        return { isBlocked: true };
      }
      
      if (!data.emailVerified) {
        throw new Error("EMAIL_NOT_VERIFIED");
      }
      
      setUser({ 
        username: data.userName || data.username, 
        userId: data.userId || data.userID, 
        roles: data.roles || [],
        email: data.email || credentials.email,
        isBlocked: false
      });
      setIsBlocked(false);
      
      const token = data.accessToken || data.access_token;
      setAccessToken(token);
      localStorage.setItem("accessToken", token);

      if (token) {
        try {
          setAvatarLoading(true);
          const avatarUrl = await getAvatarUrl(token);
          setUserAvatar(avatarUrl);
        } catch (error) {
          console.error("Failed to load user avatar:", error);
          setUserAvatar(null);
        } finally {
          setAvatarLoading(false);
        }
      }
      
      return data;
    } catch (error) {
      console.error("Login error:", error);
      if (error.message === "EMAIL_NOT_VERIFIED") {
        throw new Error("Подтвердите email перед входом");
      }
      throw error;
    }
  };

  const logout = async () => {
    try {
      if (accessToken) {
        await logoutUser(accessToken);
      }
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      setUser(null);
      setAccessToken(null);
      setUserAvatar(null);
      setAvatarLoading(false);
      setIsBlocked(false);
      localStorage.removeItem("accessToken");
      navigate("/login");
    }
  };

  const refreshAccessToken = async () => {
    try {
      const storedToken = localStorage.getItem("accessToken");
      if (!storedToken) {
        throw new Error("No access token available");
      }

      const data = await refreshToken();
      
      // Проверяем, заблокирован ли пользователь
      if (data.isBlocked) {
        setUser({ 
          username: data.userName, 
          userId: data.userId, 
          roles: data.roles || [],
          email: data.email,
          isBlocked: true,
          blockedReason: data.blockedReason,
          blockedUntil: data.blockedUntil
        });
        setIsBlocked(true);
        setAccessToken(data.accessToken);
        localStorage.setItem("accessToken", data.accessToken);
        navigate('/blocked');
        return data.accessToken;
      }
      
      setUser({ 
        username: data.userName, 
        userId: data.userId, 
        roles: data.roles || [],
        email: data.email,
        isBlocked: false
      });
      setIsBlocked(false);
      
      setAccessToken(data.accessToken);
      localStorage.setItem("accessToken", data.accessToken);
      
      return data.accessToken;
    } catch (error) {
      console.error("Token refresh failed:", error);
      logout();
      throw error;
    }
  };

  const uploadAvatar = async (file) => {
    try {
      setAvatarLoading(true);
      const result = await uploadAvatarService(accessToken, file);
      setUserAvatar(result.avatarUrl);
      return result;
    } catch (error) {
      console.error("Avatar upload error:", error);
      throw error;
    } finally {
      setAvatarLoading(false);
    }
  };

  const deleteAvatar = async () => {
    try {
      setAvatarLoading(true);
      await deleteAvatarService(accessToken);
      setUserAvatar(null);
    } catch (error) {
      console.error("Avatar deletion error:", error);
      throw error;
    } finally {
      setAvatarLoading(false);
    }
  };

  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const storedToken = localStorage.getItem("accessToken");
        if (storedToken) {
          await refreshAccessToken();
        }
      } catch (err) {
        console.log("Session not restored:", err);
      } finally {
        setIsInitialized(true);
      }
    };

    initializeAuth();
  }, []);

  useEffect(() => {
    if (!accessToken || isBlocked) return;

    const timer = setInterval(() => {
      refreshAccessToken().catch(console.error);
    }, 1000 * 60 * 4); 
    return () => clearInterval(timer);
  }, [accessToken, isBlocked]);

  useEffect(() => {
    const loadUserAvatar = async () => {
      if (user && accessToken && !user.isBlocked) {
        try {
          setAvatarLoading(true);
          const avatarUrl = await getAvatarUrl(accessToken);
          setUserAvatar(avatarUrl);
        } catch (error) {
          console.error("Failed to load user avatar:", error);
          setUserAvatar(null);
        } finally {
          setAvatarLoading(false);
        }
      }
    };

    loadUserAvatar();
  }, [user, accessToken]);

  return (
    <AuthContext.Provider
      value={{ 
        user, 
        accessToken, 
        login, 
        logout, 
        isInitialized,
        refreshAccessToken,
        userAvatar,
        avatarLoading, 
        uploadAvatar,
        deleteAvatar,
        isBlocked
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}