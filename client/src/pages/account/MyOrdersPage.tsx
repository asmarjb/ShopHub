import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Package, Clock, ChevronRight, AlertCircle } from 'lucide-react';
import { orderService, Order } from '../../services/orderService';
import { useAuth } from '../../context/AuthContext';

export const MyOrdersPage: React.FC = () => {
  const { isAuthenticated } = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        const res = await orderService.getMyOrders();
        setOrders(res.items || []);
      } catch {
        setOrders([]);
      } finally {
        setLoading(false);
      }
    };

    if (isAuthenticated) {
      fetchOrders();
    } else {
      setLoading(false);
    }
  }, [isAuthenticated]);

  if (!isAuthenticated) {
    return (
      <div className="max-w-xl mx-auto py-20 text-center space-y-4">
        <AlertCircle className="w-12 h-12 text-amber-500 mx-auto" />
        <h2 className="text-xl font-bold">Sifarişlərinizi görmək üçün daxil olun</h2>
        <Link to="/login" className="px-6 py-2 bg-indigo-600 text-white rounded-xl text-xs font-bold inline-block">
          Daxil ol
        </Link>
      </div>
    );
  }

  const getStatusBadge = (status: string) => {
    switch (status.toLowerCase()) {
      case 'pending':
        return <span className="px-2.5 py-1 text-[11px] font-bold rounded-full bg-amber-100 text-amber-800 dark:bg-amber-950/60 dark:text-amber-400">Gözləmədə</span>;
      case 'processing':
        return <span className="px-2.5 py-1 text-[11px] font-bold rounded-full bg-blue-100 text-blue-800 dark:bg-blue-950/60 dark:text-blue-400">Hazırlanır</span>;
      case 'shipped':
        return <span className="px-2.5 py-1 text-[11px] font-bold rounded-full bg-indigo-100 text-indigo-800 dark:bg-indigo-950/60 dark:text-indigo-400">Yoldadır</span>;
      case 'delivered':
        return <span className="px-2.5 py-1 text-[11px] font-bold rounded-full bg-emerald-100 text-emerald-800 dark:bg-emerald-950/60 dark:text-emerald-400">Təhvil verildi</span>;
      case 'cancelled':
        return <span className="px-2.5 py-1 text-[11px] font-bold rounded-full bg-rose-100 text-rose-800 dark:bg-rose-950/60 dark:text-rose-400">Ləğv edildi</span>;
      default:
        return <span className="px-2.5 py-1 text-[11px] font-bold rounded-full bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300">{status}</span>;
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-8">
      <div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-gray-900 dark:text-gray-100 flex items-center gap-3">
          <Package className="w-8 h-8 text-indigo-600" />
          Sifarişlərim
        </h1>
        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
          Bütün sifariş tarixçəniz və cari çatdırılma statusları
        </p>
      </div>

      {loading ? (
        <div className="space-y-4 animate-pulse">
          {[...Array(3)].map((_, i) => (
            <div key={i} className="h-28 bg-gray-200 dark:bg-gray-800 rounded-2xl" />
          ))}
        </div>
      ) : orders.length > 0 ? (
        <div className="space-y-4">
          {orders.map((order) => (
            <Link
              key={order.id}
              to={`/my-orders/${order.id}`}
              className="block p-5 sm:p-6 rounded-2xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-sm hover:border-indigo-500 transition space-y-4"
            >
              <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 pb-3 border-b border-gray-100 dark:border-gray-700">
                <div className="flex items-center gap-3">
                  <span className="font-mono font-bold text-sm text-indigo-600 dark:text-indigo-400">
                    #{order.orderNumber}
                  </span>
                  {getStatusBadge(order.status)}
                </div>
                <div className="flex items-center gap-1.5 text-xs text-gray-400">
                  <Clock className="w-3.5 h-3.5" />
                  <span>{new Date(order.createdAt).toLocaleDateString()}</span>
                </div>
              </div>

              <div className="flex items-center justify-between">
                <div>
                  <p className="text-xs text-gray-500 dark:text-gray-400">
                    {order.items?.length || 0} məhsul · Çatdırılma: {order.shippingCity}
                  </p>
                  <p className="text-base font-black text-gray-900 dark:text-gray-100 mt-1">
                    ${order.totalAmount.toFixed(2)}
                  </p>
                </div>
                <div className="flex items-center gap-1 text-xs font-bold text-indigo-600 dark:text-indigo-400">
                  Detallar <ChevronRight className="w-4 h-4" />
                </div>
              </div>
            </Link>
          ))}
        </div>
      ) : (
        <div className="text-center py-20 bg-white dark:bg-gray-800 rounded-3xl border border-gray-200 dark:border-gray-700 p-8 space-y-4">
          <Package className="w-16 h-16 text-gray-300 dark:text-gray-600 mx-auto" />
          <h2 className="text-lg font-bold">Hələlik heç bir sifarişiniz yoxdur</h2>
          <p className="text-xs text-gray-500">İlk sifarişinizi yerləşdirmək üçün kataloqa nəzər yetirin.</p>
          <div className="pt-2">
            <Link
              to="/products"
              className="px-6 py-2.5 rounded-xl bg-indigo-600 text-white font-bold text-xs inline-block"
            >
              Alış-verişə başla
            </Link>
          </div>
        </div>
      )}
    </div>
  );
};
