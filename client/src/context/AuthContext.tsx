import React, { createContext, useContext, useEffect, useState } from 'react';
import { User } from '../types';
import { authService } from '../services/authService';
import toast from 'react-hot-toast';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, pass: string, rememberMe?: boolean) => Promise<void>;
  register: (data: {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
  }) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const cached = localStorage.getItem('user_data');
      if (!cached || cached === 'undefined' || cached === 'null') return null;
      return JSON.parse(cached);
    } catch {
      localStorage.removeItem('user_data');
      return null;
    }
  });
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      const token = localStorage.getItem('access_token');
      if (token) {
        try {
          const profile = await authService.getCurrentUser();
          if (profile) {
            setUser(profile);
            localStorage.setItem('user_data', JSON.stringify(profile));
          } else {
            setUser(null);
            localStorage.removeItem('user_data');
          }
        } catch {
          setUser(null);
          localStorage.removeItem('user_data');
        }
      }
      setIsLoading(false);
    };

    initAuth();

    const handleForceLogout = () => {
      setUser(null);
      toast.error('Oturum müddəti bitdi. Zəhmət olmasa yenidən daxil olun.');
    };

    window.addEventListener('auth:logout', handleForceLogout);
    return () => window.removeEventListener('auth:logout', handleForceLogout);
  }, []);

  const login = async (email: string, pass: string, rememberMe = true) => {
    setIsLoading(true);
    try {
      const data = await authService.login(email, pass, rememberMe);
      localStorage.setItem('access_token', data.accessToken);
      localStorage.setItem('refresh_token', data.refreshToken);
      if (data.user) {
        setUser(data.user);
        localStorage.setItem('user_data', JSON.stringify(data.user));
      }
      toast.success('Xoş gəldiniz!');
    } finally {
      setIsLoading(false);
    }
  };

  const register = async (data: {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
  }) => {
    setIsLoading(true);
    try {
      await authService.register(data);
      toast.success('Hesab uğurla yaradıldı! E-poçtunuzu yoxlayın.');
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    setIsLoading(true);
    try {
      await authService.logout();
      setUser(null);
      toast.success('Çıxış edildi.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
};
