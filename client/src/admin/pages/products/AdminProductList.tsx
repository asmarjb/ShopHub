import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Plus,
  Search,
  Download,
  Trash2,
  Edit,
  ExternalLink,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';
import { adminService } from '../../services/adminService';
import { Product, PagedResult } from '../../../types';
import toast from 'react-hot-toast';

export const AdminProductList: React.FC = () => {
  const [productsData, setProductsData] = useState<PagedResult<Product> | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  const fetchProducts = async () => {
    setLoading(true);
    try {
      const data = await adminService.getProducts({
        pageNumber: page,
        pageSize: 10,
        searchTerm: searchTerm.trim() || undefined,
      });
      setProductsData(data);
    } catch {
      toast.error('Məhsullar yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [page, searchTerm]);

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`"${name}" məhsulunu silmək (soft-delete) istədiyinizdən əminsiniz?`)) return;
    try {
      await adminService.deleteProduct(id);
      toast.success('Məhsul silinmişlər bölməsinə köçürüldü.');
      fetchProducts();
    } catch {
      toast.error('Məhsul silinərkən xəta baş verdi.');
    }
  };

  const handleExportCsv = async () => {
    try {
      const blob = await adminService.exportProductsCsv();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `products_export_${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
      toast.success('CSV faylı uğurla endirildi.');
    } catch {
      toast.error('CSV ixracında xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header & Actions */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Məhsullar</h1>
          <p className="text-xs text-gray-500 mt-1">Kataloqdakı məhsulların idarə edilməsi və variantları</p>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={handleExportCsv}
            className="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-xs font-bold hover:bg-gray-50 dark:hover:bg-gray-700 flex items-center gap-1.5 transition"
          >
            <Download className="w-4 h-4" /> CSV İxrac
          </button>
          <Link
            to="/admin/products/new"
            className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
          >
            <Plus className="w-4 h-4" /> Yeni Məhsul
          </Link>
        </div>
      </div>

      {/* Search & Filter Bar */}
      <div className="flex items-center gap-3">
        <div className="relative flex-1 max-w-sm">
          <input
            type="text"
            placeholder="Ada və ya SKU-ya görə axtar..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setPage(1);
            }}
            className="w-full pl-9 pr-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800"
          />
          <Search className="w-3.5 h-3.5 text-gray-400 absolute left-3 top-3" />
        </div>
      </div>

      {/* Products Table */}
      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
              <tr>
                <th className="px-6 py-4 font-bold">Məhsul</th>
                <th className="px-6 py-4 font-bold">Kateqoriya</th>
                <th className="px-6 py-4 font-bold">SKU</th>
                <th className="px-6 py-4 font-bold">Qiymət</th>
                <th className="px-6 py-4 font-bold">Stok</th>
                <th className="px-6 py-4 font-bold text-right">Əməliyyatlar</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
              {loading ? (
                <tr>
                  <td colSpan={6} className="text-center py-10 text-gray-400">
                    Məlumatlar yüklənir...
                  </td>
                </tr>
              ) : productsData && productsData.items.length > 0 ? (
                productsData.items.map((prod) => (
                  <tr key={prod.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30 transition">
                    <td className="px-6 py-4 flex items-center gap-3">
                      <img
                        src={prod.primaryImageUrl || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=100'}
                        alt=""
                        className="w-10 h-10 rounded-xl object-cover bg-gray-100 dark:bg-gray-800"
                      />
                      <div>
                        <span className="font-bold text-gray-900 dark:text-gray-100 line-clamp-1">
                          {prod.name}
                        </span>
                        {prod.isFeatured && (
                          <span className="text-[10px] text-amber-500 font-semibold">★ Seçilmiş</span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-gray-600 dark:text-gray-400">
                      {prod.categoryName || '-'}
                    </td>
                    <td className="px-6 py-4 font-mono text-gray-500">{prod.sku}</td>
                    <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                      ${prod.basePrice.toFixed(2)}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                          prod.stockQuantity > 5
                            ? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-400'
                            : 'bg-rose-100 text-rose-800 dark:bg-rose-950/60 dark:text-rose-400'
                        }`}
                      >
                        {prod.stockQuantity} ədəd
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      <Link
                        to={`/admin/products/edit/${prod.id}`}
                        className="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg inline-block transition"
                      >
                        <Edit className="w-4 h-4" />
                      </Link>
                      <button
                        onClick={() => handleDelete(prod.id, prod.name)}
                        className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg inline-block transition"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="text-center py-10 text-gray-400">
                    Heç bir məhsul tapılmadı.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {productsData && productsData.totalPages > 1 && (
          <div className="p-4 border-t border-gray-100 dark:border-gray-800 flex items-center justify-between text-xs text-gray-500">
            <span>
              Səhifə {productsData.pageNumber} / {productsData.totalPages} ({productsData.totalCount} məhsul)
            </span>
            <div className="flex gap-2">
              <button
                disabled={!productsData.hasPreviousPage}
                onClick={() => setPage((p) => p - 1)}
                className="p-1.5 border rounded-lg disabled:opacity-40"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <button
                disabled={!productsData.hasNextPage}
                onClick={() => setPage((p) => p + 1)}
                className="p-1.5 border rounded-lg disabled:opacity-40"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
