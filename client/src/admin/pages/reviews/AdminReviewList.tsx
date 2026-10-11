import React, { useEffect, useState } from 'react';
import { Star, Check, X, Trash2 } from 'lucide-react';
import { adminService } from '../../services/adminService';
import toast from 'react-hot-toast';

export const AdminReviewList: React.FC = () => {
  const [reviews, setReviews] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  const fetchReviews = async () => {
    setLoading(true);
    try {
      const data = await adminService.getReviews();
      setReviews(data.items || []);
    } catch {
      toast.error('Rəylər yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReviews();
  }, []);

  const handleModerate = async (id: string, isApproved: boolean) => {
    try {
      await adminService.moderateReview(id, isApproved);
      toast.success(isApproved ? 'Rəy təsdiqləndi!' : 'Rəy rədd edildi.');
      fetchReviews();
    } catch {
      toast.error('Əməliyyat zamanı xəta baş verdi.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Rəyi silmək istədiyinizdən əminsiniz?')) return;
    try {
      await adminService.deleteReview(id);
      toast.success('Rəy silindi.');
      fetchReviews();
    } catch {
      toast.error('Silinərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Rəylərin Moderasiyası</h1>
        <p className="text-xs text-gray-500 mt-1">Müştərilərin yazdığı rəylərin yoxlanılması və təsdiqi</p>
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">Müştəri & Reytinq</th>
              <th className="px-6 py-4 font-bold">Rəy Mətni</th>
              <th className="px-6 py-4 font-bold">Tarix</th>
              <th className="px-6 py-4 font-bold">Vəziyyət</th>
              <th className="px-6 py-4 font-bold text-right">Moderasiya</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {reviews.map((r) => (
              <tr key={r.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                <td className="px-6 py-4">
                  <div className="font-bold text-gray-900 dark:text-gray-100">{r.userName}</div>
                  <div className="flex gap-0.5 text-amber-400 mt-1">
                    {[...Array(r.rating || 5)].map((_, i) => (
                      <Star key={i} className="w-3 h-3 fill-amber-400" />
                    ))}
                  </div>
                </td>
                <td className="px-6 py-4 max-w-xs">
                  {r.title && <p className="font-bold text-gray-900 dark:text-gray-100 mb-0.5">{r.title}</p>}
                  <p className="text-gray-500 line-clamp-2">{r.comment}</p>
                </td>
                <td className="px-6 py-4 text-gray-400 text-[11px]">
                  {new Date(r.createdAt).toLocaleDateString()}
                </td>
                <td className="px-6 py-4">
                  <span
                    className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                      r.isApproved
                        ? 'bg-emerald-100 text-emerald-800'
                        : 'bg-amber-100 text-amber-800'
                    }`}
                  >
                    {r.isApproved ? 'Təsdiqlənib' : 'Gözləmədə'}
                  </span>
                </td>
                <td className="px-6 py-4 text-right space-x-1.5">
                  {!r.isApproved && (
                    <button
                      onClick={() => handleModerate(r.id, true)}
                      title="Təsdiqlə"
                      className="p-1.5 rounded-lg bg-emerald-50 text-emerald-600 hover:bg-emerald-100 transition"
                    >
                      <Check className="w-4 h-4" />
                    </button>
                  )}
                  {r.isApproved && (
                    <button
                      onClick={() => handleModerate(r.id, false)}
                      title="Rədd et"
                      className="p-1.5 rounded-lg bg-amber-50 text-amber-600 hover:bg-amber-100 transition"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  )}
                  <button
                    onClick={() => handleDelete(r.id)}
                    title="Sil"
                    className="p-1.5 rounded-lg bg-rose-50 text-rose-600 hover:bg-rose-100 transition"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
