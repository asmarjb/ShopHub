import React, { useEffect, useState } from 'react';
import { Plus, Trash2, Edit, Ticket } from 'lucide-react';
import { adminService, CouponDto } from '../../services/adminService';
import toast from 'react-hot-toast';

export const AdminCouponList: React.FC = () => {
  const [coupons, setCoupons] = useState<CouponDto[]>([]);
  const [loading, setLoading] = useState(true);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    code: '',
    discountType: 'Percentage',
    discountValue: 10,
    minOrderAmount: 0,
    maxDiscountAmount: 50,
    startDate: new Date().toISOString().slice(0, 10),
    endDate: new Date(Date.now() + 30 * 24 * 3600 * 1000).toISOString().slice(0, 10),
    usageLimit: 100,
    isActive: true,
  });

  const fetchCoupons = async () => {
    setLoading(true);
    try {
      const data = await adminService.getCoupons();
      setCoupons(data);
    } catch {
      toast.error('Kuponlar yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCoupons();
  }, []);

  const handleOpenCreate = () => {
    setEditingId(null);
    setFormData({
      code: '',
      discountType: 'Percentage',
      discountValue: 10,
      minOrderAmount: 0,
      maxDiscountAmount: 50,
      startDate: new Date().toISOString().slice(0, 10),
      endDate: new Date(Date.now() + 30 * 24 * 3600 * 1000).toISOString().slice(0, 10),
      usageLimit: 100,
      isActive: true,
    });
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await adminService.updateCoupon(editingId, formData);
        toast.success('Kupon yeniləndi!');
      } else {
        await adminService.createCoupon(formData);
        toast.success('Yeni kupon yaradıldı!');
      }
      setIsModalOpen(false);
      fetchCoupons();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Kupon saxlanılarkən xəta baş verdi.');
    }
  };

  const handleDelete = async (id: string, code: string) => {
    if (!window.confirm(`"${code}" kuponunu silmək istədiyinizdən əminsiniz?`)) return;
    try {
      await adminService.deleteCoupon(id);
      toast.success('Kupon silindi.');
      fetchCoupons();
    } catch {
      toast.error('Kupon silinərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Kuponlar & Endirimlər</h1>
          <p className="text-xs text-gray-500 mt-1">Promokodlar və xüsusi kampaniya endirimləri</p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Yeni Kupon
        </button>
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">Kupon Kodu</th>
              <th className="px-6 py-4 font-bold">Növü & Dəyəri</th>
              <th className="px-6 py-4 font-bold">İstifadə</th>
              <th className="px-6 py-4 font-bold">Müddət</th>
              <th className="px-6 py-4 font-bold">Status</th>
              <th className="px-6 py-4 font-bold text-right">Əməliyyat</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {coupons.map((c) => (
              <tr key={c.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                <td className="px-6 py-4 font-mono font-bold text-indigo-600 dark:text-indigo-400">
                  {c.code}
                </td>
                <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                  {c.discountType === 'Percentage' ? `${c.discountValue}%` : `$${c.discountValue}`}
                </td>
                <td className="px-6 py-4 text-gray-500">
                  {c.usedCount} / {c.usageLimit || '∞'}
                </td>
                <td className="px-6 py-4 text-gray-400 text-[11px]">
                  {new Date(c.startDate).toLocaleDateString()} - {new Date(c.endDate).toLocaleDateString()}
                </td>
                <td className="px-6 py-4">
                  <span
                    className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                      c.isActive ? 'bg-emerald-100 text-emerald-800' : 'bg-gray-100 text-gray-600'
                    }`}
                  >
                    {c.isActive ? 'Aktiv' : 'Passiv'}
                  </span>
                </td>
                <td className="px-6 py-4 text-right">
                  <button
                    onClick={() => handleDelete(c.id, c.code)}
                    className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg transition"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Coupon Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSave}
            className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">Yeni Kupon</h3>

            <div>
              <label className="block text-xs font-semibold mb-1">Kupon Kodu *</label>
              <input
                type="text"
                required
                value={formData.code}
                onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                className="w-full px-4 py-2 text-xs rounded-xl border font-mono uppercase"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold mb-1">Endirim Növü</label>
                <select
                  value={formData.discountType}
                  onChange={(e) => setFormData({ ...formData, discountType: e.target.value as any })}
                  className="w-full px-4 py-2 text-xs rounded-xl border"
                >
                  <option value="Percentage">Faiz (%)</option>
                  <option value="FixedAmount">Sabit Məbləğ ($)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold mb-1">Dəyər *</label>
                <input
                  type="number"
                  required
                  value={formData.discountValue}
                  onChange={(e) => setFormData({ ...formData, discountValue: parseFloat(e.target.value) || 0 })}
                  className="w-full px-4 py-2 text-xs rounded-xl border"
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold mb-1">Başlama Tarixi</label>
                <input
                  type="date"
                  value={formData.startDate}
                  onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                  className="w-full px-4 py-2 text-xs rounded-xl border"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold mb-1">Bitmə Tarixi</label>
                <input
                  type="date"
                  value={formData.endDate}
                  onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                  className="w-full px-4 py-2 text-xs rounded-xl border"
                />
              </div>
            </div>

            <div className="flex gap-2 pt-2">
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="flex-1 py-2 text-xs rounded-xl border font-semibold"
              >
                Ləğv et
              </button>
              <button
                type="submit"
                className="flex-1 py-2 text-xs rounded-xl bg-indigo-600 text-white font-bold"
              >
                Yadda saxla
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
