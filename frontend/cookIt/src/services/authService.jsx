import {API_BASE_URL} from '../config/settings';

export async function registerUser(userData) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(userData),
  });

  if (!response.ok) {
    const errorData = await response.text();
    throw new Error(errorData || "Registration failed");
  }

  return await response.json();
}

export async function loginUser(credentials) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(credentials),
  });

  if (!response.ok) {
    let errorData;
    try {
      errorData = await response.json();
    } catch {
      errorData = await response.text();
    }
    
    const errorMessage = typeof errorData === 'string' 
      ? errorData 
      : errorData.message || "Login failed";
    
    throw new Error(errorMessage);
  }

  const data = await response.json();
  
  console.log("Login response data:", data);
  
  return {
    ...data,
    accessToken: data.accessToken || data.access_token, 
    emailVerified: data.isEmailVerified || data.emailVerified,
    roles: data.roles || [],
    isBlocked: data.isBlocked || false,
    blockedReason: data.blockedReason,
    blockedUntil: data.blockedUntil
  };
}

export async function refreshToken() {
  const response = await fetch(`${API_BASE_URL}/api/Auth/refresh`, {
    method: "POST",
    credentials: "include", 
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error("Refresh token expired or invalid");
    }
    throw new Error("Could not refresh token");
  }

  const data = await response.json();
  
  return {
    ...data,
    accessToken: data.accessToken || data.AccessToken,
    userName: data.userName || data.UserName,
    userId: data.userId || data.UserId,
    roles: data.roles || data.Roles || [],
    emailVerified: data.isEmailVerified || data.IsEmailVerified,
    isBlocked: data.isBlocked || false,
    blockedReason: data.blockedReason,
    blockedUntil: data.blockedUntil
  };
}

export const logoutUser = async (accessToken) => {
  const response = await fetch(`${API_BASE_URL}/api/Auth/logout`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`, 
    },
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Logout failed");
  }

  return true;
};

export async function confirmEmail(userId, token) {
  const url = `${API_BASE_URL}/api/Auth/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`;
  
  const response = await fetch(url, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Confirmation failed");
  }

  return await response.json();
}

export async function resendVerification(email) {
  const response = await fetch(
    `${API_BASE_URL}/api/Auth/resend-confirmation?email=${encodeURIComponent(email)}`, 
    {
      method: "GET",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
    }
  );

  if (!response.ok) {
    let errorData;
    try {
      errorData = await response.json(); 
    } catch {
      const text = await response.text();
      errorData = { message: text };
    }
    throw new Error(errorData.message || "Resend failed");
  }

  return await response.json(); 
}

export async function changePassword(accessToken, passwordData) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/change-password`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    credentials: "include",
    body: JSON.stringify(passwordData),
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Password change failed");
  }

  return await response.json();
}

export async function deleteAccount(accessToken, password) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/delete-account`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    credentials: "include",
    body: JSON.stringify({ password }),
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Account deletion failed");
  }

  return await response.json();
}

export async function uploadAvatar(accessToken, file) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/api/users/avatar`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    credentials: "include",
    body: formData,
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Avatar upload failed");
  }

  return await response.json();
}

export async function deleteAvatar(accessToken) {
  const response = await fetch(`${API_BASE_URL}/api/users/avatar`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    credentials: "include",
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Avatar deletion failed");
  }

  return await response.json();
}

export async function getAvatarUrl(accessToken) {
  const response = await fetch(`${API_BASE_URL}/api/users/avatar`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    credentials: "include",
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Failed to get avatar");
  }

  const data = await response.json();
  
  if (!data.hasAvatar) {
    return null;
  }
  
  return data.avatarUrl;
}

export async function getUserAvatarUrl(userId) {
  const response = await fetch(`${API_BASE_URL}/api/users/${userId}/avatar`, {
    method: "GET",
    credentials: "include",
  });

  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.message || "Failed to get user avatar");
  }

  const data = await response.json();
  
  if (!data.hasAvatar) {
    return null;
  }
  
  return data.avatarUrl;
}