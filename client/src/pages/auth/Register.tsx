import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { UserPlus, Lock, Mail, User, Phone } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';

export const Register: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { register } = useAuth();

  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
  });
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.password !== formData.confirmPassword) {
      toast.error('Passwords do not match!');
      return;
    }

    setLoading(true);
    try {
      await register(formData);
      toast.success('Registration successful! Welcome aboard.');
      navigate('/');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'An error occurred during registration.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[75vh] flex items-center justify-center px-4 py-16">
      <div className="w-full max-w-lg bg-white dark:bg-zinc-900 rounded-3xl border border-zinc-200 dark:border-zinc-800 shadow-2xl p-8 sm:p-10 space-y-7">
        <div className="text-center space-y-2">
          <div className="w-12 h-12 bg-black text-white dark:bg-white dark:text-black rounded-2xl flex items-center justify-center mx-auto shadow-md">
            <UserPlus className="w-5 h-5" />
          </div>
          <h1 className="text-2xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans">
            {t('auth.registerTitle')}
          </h1>
          <p className="text-xs text-zinc-500 dark:text-zinc-400 font-light">
            {t('auth.registerSubtitle')}
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                {t('auth.firstName')} *
              </label>
              <div className="relative">
                <input
                  type="text"
                  required
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  placeholder="John"
                  className="w-full pl-10 pr-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                />
                <User className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
              </div>
            </div>

            <div>
              <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                {t('auth.lastName')} *
              </label>
              <div className="relative">
                <input
                  type="text"
                  required
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  placeholder="Doe"
                  className="w-full pl-10 pr-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                />
                <User className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
              </div>
            </div>
          </div>

          <div>
            <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
              {t('auth.email')} *
            </label>
            <div className="relative">
              <input
                type="email"
                required
                name="email"
                value={formData.email}
                onChange={handleChange}
                placeholder="john.doe@example.com"
                className="w-full pl-10 pr-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
              />
              <Mail className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
            </div>
          </div>

          <div>
            <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
              {t('auth.phoneNumber')}
            </label>
            <div className="relative">
              <input
                type="tel"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleChange}
                placeholder="+1 555 019 283"
                className="w-full pl-10 pr-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
              />
              <Phone className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                {t('auth.password')} *
              </label>
              <div className="relative">
                <input
                  type="password"
                  required
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="••••••••"
                  className="w-full pl-10 pr-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                />
                <Lock className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
              </div>
            </div>

            <div>
              <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                {t('auth.confirmPassword')} *
              </label>
              <div className="relative">
                <input
                  type="password"
                  required
                  name="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  placeholder="••••••••"
                  className="w-full pl-10 pr-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                />
                <Lock className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3.5" />
              </div>
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-3.5 rounded-full bg-black text-white dark:bg-white dark:text-black font-black text-xs uppercase tracking-wider hover:opacity-85 shadow-lg transition disabled:opacity-40 mt-2"
          >
            {loading ? 'Creating Account...' : t('auth.registerTitle')}
          </button>
        </form>

        <div className="text-center pt-2 border-t border-zinc-100 dark:border-zinc-800">
          <Link
            to="/login"
            className="text-xs font-semibold text-zinc-600 dark:text-zinc-400 hover:text-black dark:hover:text-white"
          >
            {t('auth.alreadyHaveAccount')}
          </Link>
        </div>
      </div>
    </div>
  );
};
