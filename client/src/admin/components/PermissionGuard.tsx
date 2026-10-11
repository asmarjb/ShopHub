import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { AlertCircle } from 'lucide-react';

interface PermissionGuardProps {
  requiredPermission?: string;
  children?: React.ReactNode;
}

export const PermissionGuard: React.FC<PermissionGuardProps> = ({ requiredPermission, children }) => {
  const { user, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-10 h-10 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  // Admin rolu və ya tələb olunan icazə yoxlanışı
  const isAdmin = user.roles?.some((r) => r.toLowerCase() === 'admin');
  const hasPermission = requiredPermission ? user.permissions?.includes(requiredPermission) : true;

  if (!isAdmin && !hasPermission) {
    return (
      <div className="min-h-[70vh] flex items-center justify-center p-6 text-center">
        <div className="max-w-md bg-white dark:bg-gray-800 p-8 rounded-3xl border border-gray-200 dark:border-gray-700 shadow-xl space-y-4">
          <AlertCircle className="w-16 h-16 text-rose-500 mx-auto" />
          <h2 className="text-xl font-bold text-gray-900 dark:text-gray-100">Giriş Məhdudlaşdırılıb</h2>
          <p className="text-xs text-gray-500">
            Bu bölməyə daxil olmaq üçün tələb olunan icazəniz yoxdur: <code>{requiredPermission}</code>
          </p>
        </div>
      </div>
    );
  }

  return children ? <>{children}</> : <Outlet />;
};
