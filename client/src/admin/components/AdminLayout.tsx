import React, { useState } from 'react';
import { Outlet, Link } from 'react-router-dom';
import { Menu, Sun, Moon, ArrowLeft, LogOut } from 'lucide-react';
import { AdminSidebar } from './AdminSidebar';
import { useTheme } from '../../context/ThemeContext';
import { useAuth } from '../../context/AuthContext';

export const AdminLayout: React.FC = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { theme, toggleTheme } = useTheme();
  const { logout } = useAuth();

  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 flex flex-col">
      {/* Sidebar */}
      <AdminSidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />

      {/* Main Content Area */}
      <div className="lg:pl-64 flex flex-col flex-1">
        {/* Top Header */}
        <header className="sticky top-0 z-30 h-18 bg-white/90 dark:bg-zinc-950/90 backdrop-blur border-b border-zinc-200 dark:border-zinc-800 flex items-center justify-between px-6 sm:px-8">
          <div className="flex items-center gap-3">
            <button
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden p-2 rounded-lg text-zinc-600 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800"
            >
              <Menu className="w-5 h-5" />
            </button>
            <Link
              to="/"
              className="inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-zinc-500 hover:text-black dark:hover:text-white transition"
            >
              <ArrowLeft className="w-3.5 h-3.5" /> Return to Storefront
            </Link>
          </div>

          <div className="flex items-center gap-3">
            {/* Theme Toggle */}
            <button
              onClick={toggleTheme}
              title={`Switch to ${theme === 'dark' ? 'Light' : 'Dark'} theme`}
              className="p-2.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition"
            >
              {theme === 'dark' ? <Sun className="w-4 h-4 text-white" /> : <Moon className="w-4 h-4 text-black" />}
            </button>

            {/* Logout */}
            <button
              onClick={() => logout()}
              title="Sign Out"
              className="p-2.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-zinc-500 hover:text-red-600 hover:border-red-500 transition"
            >
              <LogOut className="w-4 h-4" />
            </button>
          </div>
        </header>

        {/* Page Content */}
        <main className="p-6 sm:p-10 flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  );
};
