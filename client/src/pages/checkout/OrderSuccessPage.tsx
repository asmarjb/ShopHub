import React from 'react';
import { useLocation, Link, Navigate } from 'react-router-dom';
import { CheckCircle2, ArrowRight, Package, Home } from 'lucide-react';
import { Order } from '../../services/orderService';

export const OrderSuccessPage: React.FC = () => {
  const location = useLocation();
  const order = (location.state as any)?.order as Order;

  if (!order) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="max-w-2xl mx-auto px-4 py-16 text-center space-y-6">
      <div className="w-20 h-20 rounded-full bg-emerald-50 dark:bg-emerald-950/50 text-emerald-500 flex items-center justify-center mx-auto shadow-inner">
        <CheckCircle2 className="w-12 h-12" />
      </div>

      <div className="space-y-2">
        <h1 className="text-3xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans">
          Order Confirmed!
        </h1>
        <p className="text-xs text-zinc-500 dark:text-zinc-400">
          Order Number: <span className="font-mono font-bold text-indigo-600 dark:text-indigo-400">{order.orderNumber}</span>
        </p>
      </div>

      <div className="p-7 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 shadow-sm text-left space-y-4">
        <div className="flex justify-between text-xs text-zinc-500 pb-3 border-b border-zinc-100 dark:border-zinc-800">
          <span>Amount Paid:</span>
          <span className="font-extrabold text-sm text-zinc-900 dark:text-zinc-100">${order.totalAmount.toFixed(2)}</span>
        </div>
        <div className="flex justify-between text-xs text-zinc-500 pb-3 border-b border-zinc-100 dark:border-zinc-800">
          <span>Shipping Method:</span>
          <span className="font-semibold text-zinc-800 dark:text-zinc-200">{order.shippingMethodName || 'Standard Delivery'}</span>
        </div>
        <div className="text-xs text-zinc-500">
          <span className="block mb-1 text-[11px] font-black uppercase tracking-wider text-zinc-400">Delivery Address:</span>
          <p className="font-medium text-zinc-800 dark:text-zinc-200">
            {order.shippingAddressLine1}, {order.shippingCity}, {order.shippingCountry}
          </p>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row items-center justify-center gap-3 pt-4">
        <Link
          to={`/my-orders`}
          className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-7 py-3.5 rounded-full bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-extrabold text-xs uppercase tracking-wider hover:from-indigo-500 hover:to-purple-500 shadow-lg shadow-indigo-500/20 transition"
        >
          <Package className="w-4 h-4" /> View My Orders
        </Link>
        <Link
          to="/"
          className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-7 py-3.5 rounded-full border border-zinc-200 dark:border-zinc-800 font-extrabold text-xs uppercase tracking-wider text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition"
        >
          <Home className="w-4 h-4" /> Return to Home
        </Link>
      </div>
    </div>
  );
};
