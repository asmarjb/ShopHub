import React, { useEffect, useState } from 'react';
import { RotateCcw, Check, X } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { ReturnRequest } from '../../../services/orderService';
import toast from 'react-hot-toast';

export const AdminReturnList: React.FC = () => {
  const [returns, setReturns] = useState<ReturnRequest[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchReturns = async () => {
    setLoading(true);
    try {
      const data = await adminService.getReturns();
      setReturns(data.items || []);
    } catch {
      toast.error('Qaytarma sorğuları yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReturns();
  }, []);

  const handleProcess = async (id: string, status: string) => {
    const adminNotes = window.prompt('Admin qeydi daxil edin (istəyə görə):') || undefined;
    try {
      await adminService.processReturn(id, status, adminNotes);
      toast.success(`Sorğu statusu: ${status}`);
      fetchReturns();
    } catch {
      toast.error('Sorğu işlənərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Geri Qaytarma Sorğuları</h1>
        <p className="text-xs text-gray-500 mt-1">Müştərilərin təhvil aldıqdan sonra açdığı geri qaytarma müraciətləri</p>
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">Sifariş #</th>
              <th className="px-6 py-4 font-bold">Səbəb</th>
              <th className="px-6 py-4 font-bold">Tarix</th>
              <th className="px-6 py-4 font-bold">Vəziyyət</th>
              <th className="px-6 py-4 font-bold text-right">Qərar</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {returns.map((ret) => (
              <tr key={ret.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                <td className="px-6 py-4 font-mono font-bold text-indigo-600 dark:text-indigo-400">
                  #{ret.orderNumber}
                </td>
                <td className="px-6 py-4 text-gray-600 dark:text-gray-300 max-w-xs">{ret.reason}</td>
                <td className="px-6 py-4 text-gray-400">{new Date(ret.createdAt).toLocaleDateString()}</td>
                <td className="px-6 py-4">
                  <span
                    className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                      ret.status.toLowerCase() === 'approved'
                        ? 'bg-emerald-100 text-emerald-800'
                        : ret.status.toLowerCase() === 'rejected'
                        ? 'bg-rose-100 text-rose-800'
                        : 'bg-amber-100 text-amber-800'
                    }`}
                  >
                    {ret.status}
                  </span>
                </td>
                <td className="px-6 py-4 text-right space-x-1.5">
                  {ret.status.toLowerCase() === 'pending' && (
                    <>
                      <button
                        onClick={() => handleProcess(ret.id, 'Approved')}
                        className="px-2.5 py-1 rounded-lg bg-emerald-50 text-emerald-600 hover:bg-emerald-100 font-bold transition"
                      >
                        Təsdiqlə
                      </button>
                      <button
                        onClick={() => handleProcess(ret.id, 'Rejected')}
                        className="px-2.5 py-1 rounded-lg bg-rose-50 text-rose-600 hover:bg-rose-100 font-bold transition"
                      >
                        İmtina
                      </button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
