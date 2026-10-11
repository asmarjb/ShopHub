import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { LogIn, Lock, Mail } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';

export const Login: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(true);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await login(email, password, rememberMe);
      toast.success('Signed in successfully!');
      navigate('/');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Invalid email or password.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[75vh] flex items-center justify-center px-4 py-16">
      <div className="w-full max-w-md bg-white dark:bg-zinc-900 rounded-3xl border border-zinc-200 dark:border-zinc-800 shadow-2xl p-8 sm:p-10 space-y-7">
        <div className="text-center space-y-2">
          <div className="w-12 h-12 bg-black text-white dark:bg-white dark:text-black rounded-2xl flex items-center justify-center mx-auto shadow-md">
            <LogIn className="w-5 h-5" />
          </div>
          <h1 className="text-2xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans">
            {t('auth.loginTitle')}
          </h1>
          <p className="text-xs text-zinc-500 dark:text-zinc-400 font-light">
            {t('auth.loginSubtitle')}
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
              {t('auth.email')}
            </label>
            <div className="relative">
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@example.com"
                className="w-full pl-10 pr-4 py-3 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
              />
              <Mail className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-1.5">
              <label className="text-[11px] font-black uppercase tracking-widest text-zinc-400">
                {t('auth.password')}
              </label>
              <Link
                to="/forgot-password"
                className="text-xs text-zinc-500 hover:text-black dark:hover:text-white underline font-medium"
              >
                {t('auth.forgotPassword')}
              </Link>
            </div>
            <div className="relative">
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full pl-10 pr-4 py-3 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
              />
              <Lock className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
            </div>
          </div>

          <div className="flex items-center pt-1">
            <label className="flex items-center gap-2 text-xs text-zinc-600 dark:text-zinc-400 cursor-pointer">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                className="rounded border-zinc-300 dark:border-zinc-700 text-black focus:ring-black"
              />
              <span>{t('auth.rememberMe')}</span>
            </label>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-3.5 rounded-full bg-black text-white dark:bg-white dark:text-black font-black text-xs uppercase tracking-wider hover:opacity-85 shadow-lg transition disabled:opacity-40"
          >
            {loading ? 'Authenticating...' : t('auth.loginTitle')}
          </button>
        </form>

        <div className="text-center pt-2 border-t border-zinc-100 dark:border-zinc-800">
          <Link
            to="/register"
            className="text-xs font-semibold text-zinc-600 dark:text-zinc-400 hover:text-black dark:hover:text-white"
          >
            {t('auth.dontHaveAccount')}
          </Link>
        </div>
      </div>
    </div>
  );
};
