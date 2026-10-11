import React, { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { CheckCircle2, XCircle, ArrowRight } from 'lucide-react';
import { authService } from '../../services/authService';

export const ConfirmEmail: React.FC = () => {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get('userId') || '';
  const token = searchParams.get('token') || '';

  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');

  useEffect(() => {
    const verify = async () => {
      if (!userId || !token) {
        setStatus('error');
        return;
      }
      try {
        await authService.confirmEmail(userId, token);
        setStatus('success');
      } catch {
        setStatus('error');
      }
    };

    verify();
  }, [userId, token]);

  return (
    <div className="min-h-[70vh] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl border border-gray-200 dark:border-gray-700 shadow-xl p-8 text-center space-y-6">
        {status === 'loading' && (
          <div className="space-y-4">
            <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto" />
            <h2 className="text-xl font-bold">E-poçt təsdiqlənir...</h2>
          </div>
        )}

        {status === 'success' && (
          <div className="space-y-4">
            <CheckCircle2 className="w-16 h-16 text-emerald-500 mx-auto" />
            <h2 className="text-2xl font-bold text-gray-900 dark:text-gray-100">E-poçt Təsdiqləndi!</h2>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              Hesabınız uğurla aktivləşdirildi. İndi alış-verişə başlaya bilərsiniz.
            </p>
            <div className="pt-2">
              <Link
                to="/login"
                className="inline-flex items-center gap-2 px-6 py-2.5 rounded-xl bg-indigo-600 text-white font-bold text-sm hover:bg-indigo-700 transition"
              >
                Daxil ol <ArrowRight className="w-4 h-4" />
              </Link>
            </div>
          </div>
        )}

        {status === 'error' && (
          <div className="space-y-4">
            <XCircle className="w-16 h-16 text-rose-500 mx-auto" />
            <h2 className="text-2xl font-bold text-gray-900 dark:text-gray-100">Təsdiq Baş Tutmadı</h2>
            <p className="text-xs text-gray-500 dark:text-gray-400">
              Linkin müddəti bitib və ya parametr yanlışdır.
            </p>
            <div className="pt-2">
              <Link
                to="/login"
                className="inline-flex items-center gap-2 px-6 py-2.5 rounded-xl bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 font-bold text-sm"
              >
                Girişə qayıt
              </Link>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
