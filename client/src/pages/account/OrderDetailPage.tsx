import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import {
  Package,
  Clock,
  ArrowLeft,
  Truck,
  CheckCircle,
  XCircle,
  RotateCcw,
  Ban,
} from 'lucide-react';
import { orderService, Order } from '../../services/orderService';
import toast from 'react-hot-toast';

export const OrderDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);

  // Return request modal state
  const [isReturnModalOpen, setIsReturnModalOpen] = useState(false);
  const [returnReason, setReturnReason] = useState('');
  const [submittingReturn, setSubmittingReturn] = useState(false);

  // Cancel order state
  const [cancelling, setCancelling] = useState(false);

  const fetchOrder = async () => {
    if (!id) return;
    try {
      const data = await orderService.getOrderById(id);
      setOrder(data);
    } catch {
      toast.error('Sifariş məlumatları tapılmadı.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrder();
  }, [id]);

  const handleCancelOrder = async () => {
    if (!order || !window.confirm('Sifarişi ləğv etmək istədiyinizdən əminsiniz?')) return;
    setCancelling(true);
    try {
      await orderService.cancelOrder(order.id, 'Müştəri tərəfindən ləğv edildi');
      toast.success('Sifariş ləğv edildi və stoklar bərpa olundu.');
      await fetchOrder();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Sifariş ləğv edilə bilmədi.');
    } finally {
      setCancelling(false);
    }
  };

  const handleCreateReturn = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!order || !returnReason.trim()) return;

    setSubmittingReturn(true);
    try {
      await orderService.createReturnRequest({
        orderId: order.id,
        reason: returnReason.trim(),
      });
      toast.success('Geri qaytarma sorğunuz uğurla qeydə alındı!');
      setIsReturnModalOpen(false);
      setReturnReason('');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Sorğu yaradılarkən xəta baş verdi.');
    } finally {
      setSubmittingReturn(false);
    }
  };

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto py-16 animate-pulse space-y-6">
        <div className="h-8 bg-gray-200 dark:bg-gray-800 rounded w-1/3" />
        <div className="h-40 bg-gray-200 dark:bg-gray-800 rounded-3xl" />
      </div>
    );
  }

  if (!order) {
    return (
      <div className="text-center py-20">
        <h2 className="text-xl font-bold">Sifariş tapılmadı</h2>
        <Link to="/my-orders" className="text-indigo-600 hover:underline mt-2 inline-block">
          Sifarişlərimə qayıt
        </Link>
      </div>
    );
  }

  const isPending = order.status.toLowerCase() === 'pending';
  const isDelivered = order.status.toLowerCase() === 'delivered';

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-8">
      {/* Back button & Title */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <Link
            to="/my-orders"
            className="inline-flex items-center gap-1.5 text-xs text-indigo-600 dark:text-indigo-400 font-semibold mb-2 hover:underline"
          >
            <ArrowLeft className="w-4 h-4" /> Bütün Sifarişlər
          </Link>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100 flex items-center gap-3">
            Sifariş #{order.orderNumber}
          </h1>
          <span className="text-xs text-gray-400">
            Tarix: {new Date(order.createdAt).toLocaleString()}
          </span>
        </div>

        {/* Action Buttons: Cancel or Return */}
        <div className="flex gap-2">
          {isPending && (
            <button
              onClick={handleCancelOrder}
              disabled={cancelling}
              className="px-4 py-2 rounded-xl bg-rose-50 text-rose-600 dark:bg-rose-950/60 dark:text-rose-400 text-xs font-bold hover:bg-rose-100 transition flex items-center gap-1.5 disabled:opacity-50"
            >
              <Ban className="w-4 h-4" />
              {cancelling ? 'Ləğv edilir...' : 'Sifarişi Ləğv Et'}
            </button>
          )}

          {isDelivered && (
            <button
              onClick={() => setIsReturnModalOpen(true)}
              className="px-4 py-2 rounded-xl bg-indigo-50 text-indigo-600 dark:bg-indigo-950/60 dark:text-indigo-400 text-xs font-bold hover:bg-indigo-100 transition flex items-center gap-1.5"
            >
              <RotateCcw className="w-4 h-4" />
              Geri Qaytarma Tələbi
            </button>
          )}
        </div>
      </div>

      {/* Order Status Timeline */}
      <div className="p-6 rounded-3xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm space-y-4">
        <h2 className="text-sm font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Status Zaman Oxu
        </h2>

        <div className="relative pl-6 space-y-6 before:absolute before:left-2.5 before:top-2 before:bottom-2 before:w-0.5 before:bg-indigo-200 dark:before:bg-indigo-900">
          {order.statusHistory && order.statusHistory.length > 0 ? (
            order.statusHistory.map((sh, idx) => (
              <div key={sh.id || idx} className="relative flex items-start gap-4">
                <div className="absolute -left-6 top-1 w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center ring-4 ring-white dark:ring-gray-800">
                  <CheckCircle className="w-3 h-3" />
                </div>
                <div>
                  <h4 className="font-bold text-xs text-gray-900 dark:text-gray-100 uppercase">
                    {sh.status}
                  </h4>
                  <p className="text-[11px] text-gray-400">
                    {new Date(sh.createdAt).toLocaleString()}
                  </p>
                  {sh.comment && (
                    <p className="text-xs text-gray-600 dark:text-gray-300 mt-1 italic">
                      "{sh.comment}"
                    </p>
                  )}
                </div>
              </div>
            ))
          ) : (
            <p className="text-xs text-gray-400">Cari status: {order.status}</p>
          )}
        </div>
      </div>

      {/* Items in this Order */}
      <div className="p-6 rounded-3xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm space-y-4">
        <h2 className="text-sm font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">
          Məhsullar ({order.items?.length || 0})
        </h2>

        <div className="divide-y divide-gray-100 dark:divide-gray-800">
          {order.items?.map((item) => (
            <div key={item.id} className="py-3 flex items-center justify-between text-xs">
              <div className="flex items-center gap-3">
                <div className="w-12 h-12 rounded-lg bg-gray-100 dark:bg-gray-700 flex items-center justify-center font-bold text-gray-500">
                  <Package className="w-6 h-6" />
                </div>
                <div>
                  <h4 className="font-bold text-sm text-gray-900 dark:text-gray-100">
                    {item.productName}
                  </h4>
                  {item.variantDescription && (
                    <p className="text-gray-400">{item.variantDescription}</p>
                  )}
                  <p className="text-gray-500">
                    ${item.unitPrice.toFixed(2)} × {item.quantity}
                  </p>
                </div>
              </div>
              <span className="font-black text-sm text-gray-900 dark:text-gray-100">
                ${item.totalPrice.toFixed(2)}
              </span>
            </div>
          ))}
        </div>

        {/* Pricing Summary */}
        <div className="pt-4 border-t border-gray-100 dark:border-gray-700 space-y-1.5 text-xs text-gray-500">
          <div className="flex justify-between">
            <span>Məhsul məbləği:</span>
            <span>${order.subTotal.toFixed(2)}</span>
          </div>
          <div className="flex justify-between">
            <span>Çatdırılma ({order.shippingMethodName}):</span>
            <span>${order.shippingAmount.toFixed(2)}</span>
          </div>
          {order.discountAmount > 0 && (
            <div className="flex justify-between text-emerald-600 font-semibold">
              <span>Endirim ({order.couponCode || 'Kupon'}):</span>
              <span>-${order.discountAmount.toFixed(2)}</span>
            </div>
          )}
          <div className="flex justify-between text-base font-black text-gray-900 dark:text-gray-100 pt-2 border-t border-gray-100 dark:border-gray-800">
            <span>Yekun:</span>
            <span>${order.totalAmount.toFixed(2)}</span>
          </div>
        </div>
      </div>

      {/* Return Request Modal */}
      {isReturnModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4">
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              Geri Qaytarma Müraciəti
            </h3>
            <p className="text-xs text-gray-500">
              Zəhmət olmasa məhsulu geri qaytarmaq istəməyinizin səbəbini qeyd edin:
            </p>

            <form onSubmit={handleCreateReturn} className="space-y-4">
              <textarea
                rows={4}
                required
                placeholder="Ölçü uyğun gəlmədi, zədəli gəldi və s."
                value={returnReason}
                onChange={(e) => setReturnReason(e.target.value)}
                className="w-full px-4 py-2.5 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-900"
              />

              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => setIsReturnModalOpen(false)}
                  className="flex-1 py-2 rounded-xl border border-gray-300 text-xs font-semibold"
                >
                  Ləğv et
                </button>
                <button
                  type="submit"
                  disabled={submittingReturn}
                  className="flex-1 py-2 rounded-xl bg-indigo-600 text-white text-xs font-bold hover:bg-indigo-700 disabled:opacity-50"
                >
                  {submittingReturn ? 'Göndərilir...' : 'Sorğunu Göndər'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
