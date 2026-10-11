import { api } from './api';
import { AuthResponse, ApiResponse, User } from '../types';

export const authService = {
  login: async (email: string, password: string, rememberMe = true): Promise<AuthResponse> => {
    const res = await api.post<any>('/auth/login', {
      email,
      password,
      rememberMe,
    });
    return (res.data as any)?.data ?? res.data;
  },

  register: async (data: {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
  }): Promise<{ message: string }> => {
    const res = await api.post<any>('/auth/register', data);
    return (res.data as any)?.data ?? res.data;
  },

  getCurrentUser: async (): Promise<User> => {
    const res = await api.get<any>('/auth/me');
    return (res.data as any)?.data ?? res.data;
  },

  forgotPassword: async (email: string): Promise<void> => {
    await api.post('/auth/forgot-password', { email });
  },

  resetPassword: async (data: {
    email: string;
    token: string;
    newPassword: string;
    confirmPassword: string;
  }): Promise<void> => {
    await api.post('/auth/reset-password', data);
  },

  confirmEmail: async (userId: string, token: string): Promise<void> => {
    await api.get(`/auth/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`);
  },

  logout: async (): Promise<void> => {
    try {
      const refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken) {
        await api.post('/auth/logout', { refreshToken });
      }
    } finally {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('user_data');
    }
  },
};
