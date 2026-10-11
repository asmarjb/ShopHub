import React, { useEffect, useState } from 'react';
import { RotateCcw, Trash2, AlertTriangle, Package, Layers } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { Product, Category } from '../../../types';
import toast from 'react-hot-toast';

export const AdminDeletedItems: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'products' | 'categories'>('products');
  const [deletedProducts, setDeletedProducts] = useState<Product[]>([]);
  const [deletedCategories, setDeletedCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  // Hard delete confirmation dialog
  const [hardDeleteTarget, setHardDeleteTarget] = useState<{
    id: string;
    name: string;
    type: 'product' | 'category';
  } | null>(null);
  const [hardDeleting, setHardDeleting] = useState(false);

  const fetchDeleted = async () => {
    setLoading(true);
    try {
      const [prods, cats] = await Promise.all([
        adminService.getDeletedProducts().catch(() => []),
        adminService.getDeletedCategories().catch(() => []),
      ]);
      setDeletedProducts(prods);
      setDeletedCategories(cats);
    } catch {
      toast.error('Silinmiş elementlər yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDeleted();
  }, []);

  const handleRestoreProduct = async (id: string) => {
    try {
      await adminService.restoreProduct(id);
      toast.success('Məhsul uğurla bərpa olundu!');
      fetchDeleted();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Bərpa edilərkən xəta baş verdi.');
    }
  };

  const handleRestoreCategory = async (id: string) => {
    try {
      await adminService.restoreCategory(id);
      toast.success('Kateqoriya uğurla bərpa olundu!');
      fetchDeleted();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Bərpa edilərkən xəta baş verdi.');
    }
  };

  const executeHardDelete = async () => {
    if (!hardDeleteTarget) return;
    setHardDeleting(true);
    try {
      if (hardDeleteTarget.type === 'product') {
        await adminService.hardDeleteProduct(hardDeleteTarget.id);
        toast.success('Məhsul verilənlər bazasından həmişəlik silindi.');
      } else {
        await adminService.hardDeleteCategory(hardDeleteTarget.id);
        toast.success('Kateqoriya verilənlər bazasından həmişəlik silindi.');
      }
      setHardDeleteTarget(null);
      fetchDeleted();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Həmişəlik silmə baş tutmadı.');
    } finally {
      setHardDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">
          Silinmiş Elementlər (Zibil Qutusu)
        </h1>
        <p className="text-xs text-gray-500 mt-1">
          Soft-delete edilmiş məhsul və kateqoriyaların bərpası və ya həmişəlik silinməsi
        </p>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-gray-200 dark:border-gray-800 gap-4">
        <button
          onClick={() => setActiveTab('products')}
          className={`pb-3 text-xs font-bold flex items-center gap-2 border-b-2 transition ${
            activeTab === 'products'
              ? 'border-indigo-600 text-indigo-600 dark:text-indigo-400'
              : 'border-transparent text-gray-500 hover:text-gray-700'
          }`}
        >
          <Package className="w-4 h-4" />
          Silinmiş Məhsullar ({deletedProducts.length})
        </button>

        <button
          onClick={() => setActiveTab('categories')}
          className={`pb-3 text-xs font-bold flex items-center gap-2 border-b-2 transition ${
            activeTab === 'categories'
              ? 'border-indigo-600 text-indigo-600 dark:text-indigo-400'
              : 'border-transparent text-gray-500 hover:text-gray-700'
          }`}
        >
          <Layers className="w-4 h-4" />
          Silinmiş Kateqoriyalar ({deletedCategories.length})
        </button>
      </div>

      {/* Content Table */}
      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        {activeTab === 'products' ? (
          <table className="w-full text-left text-xs">
            <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
              <tr>
                <th className="px-6 py-4 font-bold">Məhsul</th>
                <th className="px-6 py-4 font-bold">SKU</th>
                <th className="px-6 py-4 font-bold">Qiymət</th>
                <th className="px-6 py-4 font-bold text-right">Əməliyyatlar</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
              {deletedProducts.length > 0 ? (
                deletedProducts.map((p) => (
                  <tr key={p.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                    <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                      {p.name}
                    </td>
                    <td className="px-6 py-4 font-mono text-gray-500">{p.sku}</td>
                    <td className="px-6 py-4 font-bold">${p.basePrice.toFixed(2)}</td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <button
                        onClick={() => handleRestoreProduct(p.id)}
                        className="px-3 py-1.5 rounded-xl bg-emerald-50 text-emerald-600 dark:bg-emerald-950/60 dark:text-emerald-400 font-bold hover:bg-emerald-100 transition inline-flex items-center gap-1"
                      >
                        <RotateCcw className="w-3.5 h-3.5" /> Bərpa et
                      </button>
                      <button
                        onClick={() =>
                          setHardDeleteTarget({ id: p.id, name: p.name, type: 'product' })
                        }
                        className="px-3 py-1.5 rounded-xl bg-rose-50 text-rose-600 dark:bg-rose-950/60 dark:text-rose-400 font-bold hover:bg-rose-100 transition inline-flex items-center gap-1"
                      >
                        <Trash2 className="w-3.5 h-3.5" /> Həmişəlik sil
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={4} className="text-center py-10 text-gray-400">
                    Silinmiş heç bir məhsul yoxdur.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        ) : (
          <table className="w-full text-left text-xs">
            <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
              <tr>
                <th className="px-6 py-4 font-bold">Kateqoriya</th>
                <th className="px-6 py-4 font-bold">Slug</th>
                <th className="px-6 py-4 font-bold text-right">Əməliyyatlar</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
              {deletedCategories.length > 0 ? (
                deletedCategories.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                    <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                      {c.name}
                    </td>
                    <td className="px-6 py-4 font-mono text-gray-500">{c.slug}</td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <button
                        onClick={() => handleRestoreCategory(c.id)}
                        className="px-3 py-1.5 rounded-xl bg-emerald-50 text-emerald-600 dark:bg-emerald-950/60 dark:text-emerald-400 font-bold hover:bg-emerald-100 transition inline-flex items-center gap-1"
                      >
                        <RotateCcw className="w-3.5 h-3.5" /> Bərpa et
                      </button>
                      <button
                        onClick={() =>
                          setHardDeleteTarget({ id: c.id, name: c.name, type: 'category' })
                        }
                        className="px-3 py-1.5 rounded-xl bg-rose-50 text-rose-600 dark:bg-rose-950/60 dark:text-rose-400 font-bold hover:bg-rose-100 transition inline-flex items-center gap-1"
                      >
                        <Trash2 className="w-3.5 h-3.5" /> Həmişəlik sil
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={3} className="text-center py-10 text-gray-400">
                    Silinmiş heç bir kateqoriya yoxdur.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {/* Hard Delete Confirmation Modal */}
      {hardDeleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4">
            <div className="flex items-center gap-3 text-rose-600">
              <AlertTriangle className="w-8 h-8 flex-shrink-0" />
              <div>
                <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
                  Həmişəlik Silinsin? (Hard Delete)
                </h3>
                <span className="text-[11px] text-rose-500 font-semibold">Bu əməliyyat geri qaytarıla bilməz!</span>
              </div>
            </div>

            <p className="text-xs text-gray-500 leading-relaxed">
              <strong>"{hardDeleteTarget.name}"</strong> elementi və onunla bağlı bütün qeydlər
              verilənlər bazasından birdəfəlik silinəcəkdir.
            </p>

            <div className="flex gap-2 pt-2">
              <button
                type="button"
                onClick={() => setHardDeleteTarget(null)}
                className="flex-1 py-2 text-xs rounded-xl border font-semibold"
              >
                İmtina et
              </button>
              <button
                type="button"
                disabled={hardDeleting}
                onClick={executeHardDelete}
                className="flex-1 py-2 text-xs rounded-xl bg-rose-600 text-white font-bold disabled:opacity-50"
              >
                {hardDeleting ? 'Silinir...' : 'Bəli, Həmişəlik Sil'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
