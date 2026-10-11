import React, { useEffect, useState } from 'react';
import { Activity, Plus, ArrowUpRight, ArrowDownLeft } from 'lucide-react';
import { adminService, StockMovementDto } from '../../services/adminService';
import { PagedResult, Product } from '../../../types';
import toast from 'react-hot-toast';

export const AdminStockMovements: React.FC = () => {
  const [movementsData, setMovementsData] = useState<PagedResult<StockMovementDto>>({
    items: [],
    pageNumber: 1,
    pageSize: 15,
    totalCount: 0,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
  });
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  // Manual Adjustment Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [quantityChange, setQuantityChange] = useState(5);
  const [comment, setComment] = useState('Anbara yeni partiya daxil oldu');
  const [adjusting, setAdjusting] = useState(false);

  const fetchMovements = async () => {
    setLoading(true);
    try {
      const [mv, pr] = await Promise.all([
        adminService.getStockMovements(),
        adminService.getProducts({ pageSize: 50 }),
      ]);
      setMovementsData(mv);
      setProducts(pr.items || []);
      if (pr.items?.length > 0) {
        setSelectedProductId(pr.items[0].id);
      }
    } catch {
      toast.error('Stok hərəkətləri yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMovements();
  }, []);

  const handleAdjustStock = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProductId) return;

    setAdjusting(true);
    try {
      await adminService.adjustStock({
        productId: selectedProductId,
        quantityChange,
        comment,
      });
      toast.success('Stok miqdarı uğurla tənzimləndi!');
      setIsModalOpen(false);
      fetchMovements();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Stok tənzimlənərkən xəta baş verdi.');
    } finally {
      setAdjusting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Stok Hərəkətləri</h1>
          <p className="text-xs text-gray-500 mt-1">Giriş, çıxış, satış və ləğv zamanı stokun dəyişmə jurnalı</p>
        </div>

        <button
          onClick={() => setIsModalOpen(true)}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Əllə Stok Düzəlişi
        </button>
      </div>

      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">Məhsul</th>
              <th className="px-6 py-4 font-bold">Dəyişiklik</th>
              <th className="px-6 py-4 font-bold">Əməliyyat Növü</th>
              <th className="px-6 py-4 font-bold">Qeyd / Səbəb</th>
              <th className="px-6 py-4 font-bold">Tarix</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {loading ? (
              <tr>
                <td colSpan={5} className="text-center py-10 text-gray-400">Yüklənir...</td>
              </tr>
            ) : movementsData.items.length > 0 ? (
              movementsData.items.map((m) => (
                <tr key={m.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                  <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                    {m.productName}
                  </td>
                  <td className="px-6 py-4">
                    <span
                      className={`font-black flex items-center gap-1 ${
                        m.quantityChange > 0 ? 'text-emerald-500' : 'text-rose-500'
                      }`}
                    >
                      {m.quantityChange > 0 ? <ArrowUpRight className="w-3.5 h-3.5" /> : <ArrowDownLeft className="w-3.5 h-3.5" />}
                      {m.quantityChange > 0 ? `+${m.quantityChange}` : m.quantityChange}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="font-mono text-[10px] px-2 py-0.5 rounded bg-gray-100 dark:bg-gray-800 font-semibold">
                      {m.type}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-gray-500 italic">{m.comment || '-'}</td>
                  <td className="px-6 py-4 text-gray-400">{new Date(m.createdAt).toLocaleString()}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={5} className="text-center py-10 text-gray-400">Heç bir hərəkət qeydə alınmayıb.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Manual Stock Adjustment Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleAdjustStock}
            className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              Əllə Stok Düzəlişi (Manual Adjustment)
            </h3>

            <div>
              <label className="block text-xs font-semibold mb-1">Məhsul Seçin *</label>
              <select
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              >
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name} (Stok: {p.stockQuantity})
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">
                Say Dəyişikliyi (+ artır, - azalt) *
              </label>
              <input
                type="number"
                required
                value={quantityChange}
                onChange={(e) => setQuantityChange(parseInt(e.target.value, 10) || 0)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Qeyd / İzah *</label>
              <input
                type="text"
                required
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              />
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
                disabled={adjusting}
                className="flex-1 py-2 text-xs rounded-xl bg-indigo-600 text-white font-bold disabled:opacity-50"
              >
                {adjusting ? 'Yenilənir...' : 'Təsdiqlə'}
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
