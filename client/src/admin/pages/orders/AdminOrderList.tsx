import React, { useEffect, useState } from 'react';
import { Download, Search, Eye, ChevronLeft, ChevronRight, CheckCircle2 } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { Order } from '../../../services/orderService';
import { PagedResult } from '../../../types';
import toast from 'react-hot-toast';

export const AdminOrderList: React.FC = () => {
  const [ordersData, setOrdersData] = useState<PagedResult<Order> | null>(null);
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  // Status Change Modal State
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
  const [newStatus, setNewStatus] = useState('');
  const [comment, setComment] = useState('');
  const [updating, setUpdating] = useState(false);

  const fetchOrders = async () => {
    setLoading(true);
    try {
      const data = await adminService.getOrders({
        pageNumber: page,
        pageSize: 10,
        status: statusFilter || undefined,
      });
      setOrdersData(data);
    } catch {
      toast.error('Sifarişlər yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrders();
  }, [page, statusFilter]);

  const handleExportCsv = async () => {
    try {
      const blob = await adminService.exportOrdersCsv();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `orders_export_${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
      toast.success('Sifarişlər CSV kimi endirildi.');
    } catch {
      toast.error('İxrac zamanı xəta baş verdi.');
    }
  };

  const handleUpdateStatus = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedOrder || !newStatus) return;

    setUpdating(true);
    try {
      await adminService.updateOrderStatus(selectedOrder.id, newStatus, comment || undefined);
      toast.success(`Sifariş #${selectedOrder.orderNumber} statusu yeniləndi!`);
      setSelectedOrder(null);
      setComment('');
      fetchOrders();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Status dəyişdirilə bilmədi.');
    } finally {
      setUpdating(false);
    }
  };

  const getStatusBadge = (st: string) => {
    switch (st.toLowerCase()) {
      case 'pending':
        return <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800 dark:bg-amber-950/60 dark:text-amber-400">Gözləmədə</span>;
      case 'processing':
        return <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-blue-100 text-blue-800 dark:bg-blue-950/60 dark:text-blue-400">Hazırlanır</span>;
      case 'shipped':
        return <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-indigo-100 text-indigo-800 dark:bg-indigo-950/60 dark:text-indigo-400">Yoldadır</span>;
      case 'delivered':
        return <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-400">Təhvil verildi</span>;
      case 'cancelled':
        return <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-rose-100 text-rose-800 dark:bg-rose-950/60 dark:text-rose-400">Ləğv edildi</span>;
      default:
        return <span className="px-2 py-0.5 rounded-full text-[10px] font-bold bg-gray-100 text-gray-800">{st}</span>;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Sifarişlər</h1>
          <p className="text-xs text-gray-500 mt-1">Bütün müştəri sifarişləri, statuslar və çatdırılma</p>
        </div>

        <button
          onClick={handleExportCsv}
          className="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-xs font-bold hover:bg-gray-50 flex items-center gap-1.5 transition"
        >
          <Download className="w-4 h-4" /> CSV İxrac
        </button>
      </div>

      {/* Filter Bar */}
      <div className="flex gap-3">
        <select
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setPage(1);
          }}
          className="px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800"
        >
          <option value="">Bütün Statuslar</option>
          <option value="Pending">Gözləmədə (Pending)</option>
          <option value="Processing">Hazırlanır (Processing)</option>
          <option value="Shipped">Yoldadır (Shipped)</option>
          <option value="Delivered">Təhvil verildi (Delivered)</option>
          <option value="Cancelled">Ləğv edildi (Cancelled)</option>
        </select>
      </div>

      {/* Orders Table */}
      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">Sifariş Nömrəsi</th>
              <th className="px-6 py-4 font-bold">Tarix</th>
              <th className="px-6 py-4 font-bold">Məbləğ</th>
              <th className="px-6 py-4 font-bold">Çatdırılma Şəhəri</th>
              <th className="px-6 py-4 font-bold">Status</th>
              <th className="px-6 py-4 font-bold text-right">Əməliyyat</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {loading ? (
              <tr>
                <td colSpan={6} className="text-center py-10 text-gray-400">Yüklənir...</td>
              </tr>
            ) : ordersData && ordersData.items.length > 0 ? (
              ordersData.items.map((ord) => (
                <tr key={ord.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                  <td className="px-6 py-4 font-mono font-bold text-indigo-600 dark:text-indigo-400">
                    #{ord.orderNumber}
                  </td>
                  <td className="px-6 py-4 text-gray-500">
                    {new Date(ord.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 font-bold">${ord.totalAmount.toFixed(2)}</td>
                  <td className="px-6 py-4 text-gray-600 dark:text-gray-300">
                    {ord.shippingCity}
                  </td>
                  <td className="px-6 py-4">{getStatusBadge(ord.status)}</td>
                  <td className="px-6 py-4 text-right">
                    <button
                      onClick={() => {
                        setSelectedOrder(ord);
                        setNewStatus(ord.status);
                      }}
                      className="px-3 py-1.5 rounded-xl bg-indigo-50 text-indigo-600 dark:bg-indigo-950/60 dark:text-indigo-400 font-bold hover:bg-indigo-100 transition inline-flex items-center gap-1"
                    >
                      <Eye className="w-3.5 h-3.5" /> İdarə et
                    </button>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={6} className="text-center py-10 text-gray-400">Heç bir sifariş tapılmadı.</td>
              </tr>
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {ordersData && ordersData.totalPages > 1 && (
          <div className="p-4 border-t border-gray-100 dark:border-gray-800 flex items-center justify-between text-xs text-gray-500">
            <span>Səhifə {ordersData.pageNumber} / {ordersData.totalPages}</span>
            <div className="flex gap-2">
              <button
                disabled={!ordersData.hasPreviousPage}
                onClick={() => setPage((p) => p - 1)}
                className="p-1.5 border rounded-lg disabled:opacity-40"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>
              <button
                disabled={!ordersData.hasNextPage}
                onClick={() => setPage((p) => p + 1)}
                className="p-1.5 border rounded-lg disabled:opacity-40"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Status Change & Details Modal */}
      {selectedOrder && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="w-full max-w-lg bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4">
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              Sifariş #{selectedOrder.orderNumber} Detalları və Statusu
            </h3>

            {/* Items snippet */}
            <div className="p-3 rounded-2xl bg-gray-50 dark:bg-gray-900 max-h-40 overflow-y-auto space-y-2 text-xs">
              {selectedOrder.items?.map((it) => (
                <div key={it.id} className="flex justify-between">
                  <span>{it.productName} × {it.quantity}</span>
                  <span className="font-bold">${it.totalPrice.toFixed(2)}</span>
                </div>
              ))}
            </div>

            <form onSubmit={handleUpdateStatus} className="space-y-4 pt-2">
              <div>
                <label className="block text-xs font-semibold mb-1">Yeni Status Seçin *</label>
                <select
                  value={newStatus}
                  onChange={(e) => setNewStatus(e.target.value)}
                  className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
                >
                  <option value="Pending">Gözləmədə (Pending)</option>
                  <option value="Processing">Hazırlanır (Processing)</option>
                  <option value="Shipped">Yoldadır (Shipped)</option>
                  <option value="Delivered">Təhvil verildi (Delivered)</option>
                  <option value="Cancelled">Ləğv edildi (Cancelled)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold mb-1">Status Dəyişikliyi Üçün Qeyd</label>
                <input
                  type="text"
                  placeholder="Məsələn: Kuryerə təhvil verildi..."
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
                />
              </div>

              <div className="flex gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setSelectedOrder(null)}
                  className="flex-1 py-2 text-xs rounded-xl border font-semibold"
                >
                  Bağla
                </button>
                <button
                  type="submit"
                  disabled={updating}
                  className="flex-1 py-2 text-xs rounded-xl bg-indigo-600 text-white font-bold disabled:opacity-50"
                >
                  {updating ? 'Yenilənir...' : 'Statusu Yenilə'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
