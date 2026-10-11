import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { KeyRound, Mail, ArrowLeft } from 'lucide-react';
import { authService } from '../../services/authService';
import toast from 'react-hot-toast';

export const ForgotPassword: React.FC = () => {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await authService.forgotPassword(email);
      setSubmitted(true);
      toast.success('Şifrə sıfırlama linki e-poçt ünvanınıza göndərildi!');
    } catch {
      toast.error('Əməliyyat zamanı xəta baş verdi.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[75vh] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl border border-gray-200 dark:border-gray-700 shadow-xl p-8 space-y-6">
        <div className="text-center space-y-2">
          <div className="w-12 h-12 bg-indigo-50 dark:bg-indigo-950/50 text-indigo-600 rounded-2xl flex items-center justify-center mx-auto">
            <KeyRound className="w-6 h-6" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100">
            {t('auth.resetPasswordTitle')}
          </h1>
          <p className="text-xs text-gray-500 dark:text-gray-400">
            Qeydiyyatdan keçdiyiniz e-poçt ünvanını daxil edin
          </p>
        </div>

        {submitted ? (
          <div className="text-center space-y-4 py-4">
            <p className="text-sm text-gray-600 dark:text-gray-300">
              Şifrə sıfırlama təlimatları <strong>{email}</strong> ünvanına göndərildi.
            </p>
            <Link
              to="/login"
              className="inline-flex items-center gap-2 text-xs font-bold text-indigo-600 dark:text-indigo-400 hover:underline"
            >
              <ArrowLeft className="w-4 h-4" /> Giriş səhifəsinə qayıt
            </Link>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-bold uppercase tracking-wider text-gray-600 dark:text-gray-400 mb-1.5">
                {t('auth.email')}
              </label>
              <div className="relative">
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="nümunə@test.com"
                  className="w-full pl-10 pr-4 py-2.5 text-sm rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-900"
                />
                <Mail className="w-4 h-4 text-gray-400 absolute left-3.5 top-3.5" />
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-3 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-sm shadow-lg shadow-indigo-600/30 transition active:scale-98 disabled:opacity-50"
            >
              {loading ? 'Göndərilir...' : t('auth.sendResetLink')}
            </button>

            <div className="text-center pt-2">
              <Link
                to="/login"
                className="inline-flex items-center gap-1.5 text-xs text-gray-600 dark:text-gray-400 hover:text-indigo-600 dark:hover:text-indigo-400"
              >
                <ArrowLeft className="w-3.5 h-3.5" /> Daxil ol
              </Link>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
