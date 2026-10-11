import React, { useEffect, useState } from 'react';
import { Plus, Trash2, Edit, Truck } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { ShippingMethod } from '../../../services/orderService';
import toast from 'react-hot-toast';

export const AdminShippingList: React.FC = () => {
  const [methods, setMethods] = useState<ShippingMethod[]>([]);
  const [loading, setLoading] = useState(true);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState(5.0);
  const [estimatedDeliveryDays, setEstimatedDeliveryDays] = useState(2);
  const [isActive, setIsActive] = useState(true);

  const fetchMethods = async () => {
    setLoading(true);
    try {
      const data = await adminService.getShippingMethods();
      setMethods(data);
    } catch {
      toast.error('Çatdırılma metodları yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMethods();
  }, []);

  const handleOpenCreate = () => {
    setEditingId(null);
    setName('');
    setDescription('');
    setPrice(5.0);
    setEstimatedDeliveryDays(2);
    setIsActive(true);
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = { name, description, price, estimatedDeliveryDays, isActive };
      if (editingId) {
        await adminService.updateShippingMethod(editingId, payload);
        toast.success('Çatdırılma metodu yeniləndi!');
      } else {
        await adminService.createShippingMethod(payload);
        toast.success('Yeni çatdırılma metodu yaradıldı!');
      }
      setIsModalOpen(false);
      fetchMethods();
    } catch {
      toast.error('Əməliyyat zamanı xəta baş verdi.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Metodu silmək istədiyinizdən əminsiniz?')) return;
    try {
      await adminService.deleteShippingMethod(id);
      toast.success('Çatdırılma metodu silindi.');
      fetchMethods();
    } catch {
      toast.error('Silinərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Çatdırılma Metodları</h1>
          <p className="text-xs text-gray-500 mt-1">Kuryer xidmətləri, qiymətlər və çatdırılma müddəti</p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Yeni Metod
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {methods.map((m) => (
          <div
            key={m.id}
            className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm flex flex-col justify-between space-y-4"
          >
            <div>
              <div className="flex items-center justify-between mb-1">
                <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">{m.name}</h3>
                <span className="font-black text-indigo-600 dark:text-indigo-400">${m.price.toFixed(2)}</span>
              </div>
              <p className="text-xs text-gray-400 mt-1">{m.description}</p>
              <span className="text-[11px] text-gray-500 block mt-3">
                Çatdırılma: {m.estimatedDeliveryDays} iş günü
              </span>
            </div>

            <div className="flex items-center justify-end gap-2 pt-3 border-t border-gray-100 dark:border-gray-800">
              <button
                onClick={() => handleDelete(m.id)}
                className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg transition"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSave}
            className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">Yeni Çatdırılma Metodu</h3>

            <div>
              <label className="block text-xs font-semibold mb-1">Metodun Adı *</label>
              <input
                type="text"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Təsvir</label>
              <input
                type="text"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border"
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-semibold mb-1">Qiymət ($) *</label>
                <input
                  type="number"
                  step="0.01"
                  required
                  value={price}
                  onChange={(e) => setPrice(parseFloat(e.target.value) || 0)}
                  className="w-full px-4 py-2 text-xs rounded-xl border"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold mb-1">Müddət (gün) *</label>
                <input
                  type="number"
                  required
                  value={estimatedDeliveryDays}
                  onChange={(e) => setEstimatedDeliveryDays(parseInt(e.target.value, 10) || 1)}
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
