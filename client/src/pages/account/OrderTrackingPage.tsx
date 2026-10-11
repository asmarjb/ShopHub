import React, { useState } from 'react';
import { Search, Package, Clock, CheckCircle } from 'lucide-react';
import { orderService, Order } from '../../services/orderService';
import toast from 'react-hot-toast';

export const OrderTrackingPage: React.FC = () => {
  const [orderNumber, setOrderNumber] = useState('');
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(false);

  const handleTrack = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!orderNumber.trim()) return;

    setLoading(true);
    try {
      const data = await orderService.trackOrderByNumber(orderNumber.trim());
      setOrder(data);
    } catch {
      toast.error('Göstərilən nömrə ilə sifariş tapılmadı.');
      setOrder(null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto px-4 py-16 space-y-8">
      <div className="text-center space-y-2">
        <div className="w-16 h-16 rounded-2xl bg-indigo-50 dark:bg-indigo-950/50 text-indigo-600 flex items-center justify-center mx-auto">
          <Package className="w-8 h-8" />
        </div>
        <h1 className="text-3xl font-extrabold text-gray-900 dark:text-gray-100">
          Sifariş İzləmə
        </h1>
        <p className="text-xs text-gray-500 max-w-sm mx-auto">
          Sifariş nömrənizi daxil edərək cari vəziyyətini real vaxtda izləyin
        </p>
      </div>

      {/* Tracking Search Input */}
      <form onSubmit={handleTrack} className="flex gap-2 max-w-md mx-auto">
        <input
          type="text"
          required
          placeholder="Məsələn: ORD-2026-1001"
          value={orderNumber}
          onChange={(e) => setOrderNumber(e.target.value)}
          className="flex-1 px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm font-mono"
        />
        <button
          type="submit"
          disabled={loading}
          className="px-6 py-3 rounded-2xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-sm flex items-center gap-2 shadow-lg shadow-indigo-600/30 transition disabled:opacity-50"
        >
          <Search className="w-4 h-4" />
          {loading ? '...' : 'İzlə'}
        </button>
      </form>

      {/* Result Display */}
      {order && (
        <div className="p-6 rounded-3xl bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-xl space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between pb-4 border-b border-gray-100 dark:border-gray-700 gap-2">
            <div>
              <span className="text-xs text-gray-400">Sifariş Nömrəsi:</span>
              <h3 className="font-mono font-black text-lg text-indigo-600 dark:text-indigo-400">
                #{order.orderNumber}
              </h3>
            </div>
            <div>
              <span className="text-xs text-gray-400">Cari Status:</span>
              <p className="font-bold text-sm text-emerald-600 dark:text-emerald-400 uppercase">
                {order.status}
              </p>
            </div>
          </div>

          {/* Timeline */}
          <div className="space-y-4">
            <h4 className="text-xs font-bold uppercase tracking-wider text-gray-500">
              Çatdırılma Mərhələləri
            </h4>
            <div className="relative pl-6 space-y-4 before:absolute before:left-2.5 before:top-2 before:bottom-2 before:w-0.5 before:bg-indigo-100 dark:before:bg-indigo-900">
              {order.statusHistory?.map((sh, idx) => (
                <div key={idx} className="relative flex items-start gap-3">
                  <div className="absolute -left-6 top-1 w-5 h-5 rounded-full bg-indigo-600 text-white flex items-center justify-center">
                    <CheckCircle className="w-3 h-3" />
                  </div>
                  <div>
                    <h5 className="font-bold text-xs uppercase text-gray-900 dark:text-gray-100">
                      {sh.status}
                    </h5>
                    <span className="text-[10px] text-gray-400">
                      {new Date(sh.createdAt).toLocaleString()}
                    </span>
                    {sh.comment && (
                      <p className="text-xs text-gray-600 dark:text-gray-300 mt-0.5 italic">
                        "{sh.comment}"
                      </p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
