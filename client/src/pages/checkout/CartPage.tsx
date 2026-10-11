import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ShoppingBag, Trash2, ArrowRight, Tag, ShieldCheck } from 'lucide-react';
import { useCart } from '../../context/CartContext';
import { cartService } from '../../services/cartService';
import toast from 'react-hot-toast';

export const CartPage: React.FC = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { cart, updateQuantity, removeItem, clearCart } = useCart();

  const [couponInput, setCouponInput] = useState('');
  const [couponDiscount, setCouponDiscount] = useState<{
    code: string;
    discountAmount: number;
    finalTotal: number;
  } | null>(null);
  const [validatingCoupon, setValidatingCoupon] = useState(false);

  const subTotal = cart?.subTotal || 0;

  const handleApplyCoupon = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!couponInput.trim()) return;

    setValidatingCoupon(true);
    try {
      const res = await cartService.validateCoupon(couponInput.trim(), subTotal);
      setCouponDiscount({
        code: res.code,
        discountAmount: res.discountAmount,
        finalTotal: res.finalTotal,
      });
      toast.success(`Coupon applied! $${res.discountAmount.toFixed(2)} discount deducted.`);
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Invalid or expired promotional code.');
      setCouponDiscount(null);
    } finally {
      setValidatingCoupon(false);
    }
  };

  const finalTotal = couponDiscount ? couponDiscount.finalTotal : subTotal;
  const totalDiscount = (cart?.discountAmount || 0) + (couponDiscount?.discountAmount || 0);

  if (!cart || !cart.items || cart.items.length === 0) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-24 text-center space-y-6">
        <div className="w-20 h-20 rounded-full bg-zinc-100 dark:bg-zinc-800 text-zinc-900 dark:text-zinc-100 flex items-center justify-center mx-auto">
          <ShoppingBag className="w-10 h-10" />
        </div>
        <h1 className="text-3xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100">{t('cart.empty')}</h1>
        <p className="text-xs text-zinc-500 dark:text-zinc-400 max-w-sm mx-auto font-light leading-relaxed">{t('cart.emptySub')}</p>
        <div className="pt-4">
          <Link
            to="/products"
            className="inline-flex items-center gap-2 px-8 py-3.5 rounded-full bg-black text-white dark:bg-white dark:text-black font-extrabold text-xs uppercase tracking-wider hover:opacity-85 transition"
          >
            {t('cart.startShopping')} <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-8">
      <div className="flex items-center justify-between border-b border-zinc-200 dark:border-zinc-800 pb-5">
        <h1 className="text-3xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans">
          {t('cart.title')} ({cart.items.length})
        </h1>
        <button
          onClick={clearCart}
          className="text-xs text-zinc-500 hover:text-black dark:hover:text-white flex items-center gap-1 font-semibold uppercase tracking-wider transition-colors"
        >
          <Trash2 className="w-3.5 h-3.5" /> Clear Bag
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-10">
        {/* Cart Items List */}
        <div className="lg:col-span-2 space-y-4">
          {cart.items.map((item) => (
            <div
              key={item.id}
              className="p-5 sm:p-6 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-5 transition-shadow hover:shadow-md"
            >
              <div className="flex items-center gap-5">
                <div className="w-20 h-20 rounded-2xl bg-zinc-100 dark:bg-zinc-800 overflow-hidden flex-shrink-0 border border-zinc-200 dark:border-zinc-800">
                  <img
                    src={item.imageUrl || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=200&auto=format&fit=crop&q=60'}
                    alt={item.productName}
                    className="w-full h-full object-cover"
                  />
                </div>
                <div>
                  <h3 className="font-extrabold text-sm sm:text-base text-zinc-900 dark:text-zinc-100">
                    {item.productName}
                  </h3>
                  {item.variantName && (
                    <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-0.5">
                      {item.variantName}
                    </p>
                  )}
                  <span className="text-xs font-mono text-zinc-400 block mt-1">
                    SKU: {item.sku}
                  </span>
                  <span className="text-sm font-black text-zinc-900 dark:text-zinc-100 mt-1 block">
                    ${item.unitPrice.toFixed(2)}
                  </span>
                </div>
              </div>

              <div className="flex items-center justify-between sm:justify-end w-full sm:w-auto gap-5">
                {/* Quantity Controls */}
                <div className="flex items-center border border-zinc-200 dark:border-zinc-800 rounded-xl bg-zinc-50 dark:bg-zinc-950">
                  <button
                    onClick={() => updateQuantity(item.id, Math.max(1, item.quantity - 1))}
                    className="px-3 py-1.5 text-zinc-600 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-800 rounded-l-xl font-bold"
                  >
                    -
                  </button>
                  <span className="px-3 py-1 text-xs font-black">{item.quantity}</span>
                  <button
                    onClick={() => updateQuantity(item.id, item.quantity + 1)}
                    className="px-3 py-1.5 text-zinc-600 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-800 rounded-r-xl font-bold"
                  >
                    +
                  </button>
                </div>

                <div className="text-right">
                  <span className="font-black text-sm text-zinc-900 dark:text-zinc-100 block">
                    ${(item.unitPrice * item.quantity).toFixed(2)}
                  </span>
                </div>

                <button
                  onClick={() => removeItem(item.id)}
                  className="p-2 text-zinc-400 hover:text-rose-500 rounded-lg transition-colors"
                  title="Remove item"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          ))}
        </div>

        {/* Order Summary Sidebar */}
        <div className="space-y-6">
          <div className="p-6 sm:p-7 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 space-y-6 shadow-sm hover:border-indigo-300 dark:hover:border-indigo-800/80 transition-colors">
            <h2 className="text-lg font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 border-b border-zinc-100 dark:border-zinc-800 pb-4">
              Order Summary
            </h2>

            {/* Promo Code Input */}
            <form onSubmit={handleApplyCoupon} className="space-y-2">
              <label className="text-[11px] font-black uppercase tracking-widest text-indigo-600 dark:text-indigo-400 block">
                {t('cart.couponCode')}
              </label>
              <div className="flex gap-2">
                <div className="relative flex-1">
                  <input
                    type="text"
                    value={couponInput}
                    onChange={(e) => setCouponInput(e.target.value.toUpperCase())}
                    placeholder="e.g. WELCOME10"
                    className="w-full pl-9 pr-3 py-2 text-xs uppercase font-mono font-bold rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 placeholder-zinc-400 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                  <Tag className="w-3.5 h-3.5 text-indigo-500 absolute left-3 top-2.5" />
                </div>
                <button
                  type="submit"
                  disabled={validatingCoupon || !couponInput.trim()}
                  className="px-4 py-2 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 text-white text-xs font-bold uppercase tracking-wider hover:opacity-95 shadow-md shadow-indigo-500/25 transition disabled:opacity-40"
                >
                  {validatingCoupon ? '...' : t('cart.applyCoupon')}
                </button>
              </div>
              {couponDiscount && (
                <div className="p-2.5 rounded-xl bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800 text-xs text-emerald-700 dark:text-emerald-400 font-medium">
                  Applied: <span className="font-mono font-bold">{couponDiscount.code}</span> (-${couponDiscount.discountAmount.toFixed(2)})
                </div>
              )}
            </form>

            {/* Price Calculations */}
            <div className="space-y-3 pt-4 border-t border-zinc-100 dark:border-zinc-800 text-xs">
              <div className="flex justify-between text-zinc-500 dark:text-zinc-400 font-medium">
                <span>{t('cart.subtotal')}</span>
                <span className="font-bold text-zinc-900 dark:text-zinc-100">${subTotal.toFixed(2)}</span>
              </div>

              {totalDiscount > 0 && (
                <div className="flex justify-between text-emerald-600 dark:text-emerald-400 font-bold">
                  <span>{t('cart.discount')}</span>
                  <span>-${totalDiscount.toFixed(2)}</span>
                </div>
              )}

              <div className="flex justify-between text-zinc-500 dark:text-zinc-400 font-medium">
                <span>Estimated Shipping</span>
                <span className="text-emerald-600 dark:text-emerald-400 font-semibold">Free Express</span>
              </div>

              <div className="border-t border-zinc-200 dark:border-zinc-800 pt-4 flex justify-between items-baseline">
                <span className="text-sm font-black uppercase tracking-wider text-zinc-900 dark:text-zinc-100">
                  {t('cart.total')}
                </span>
                <span className="text-2xl font-black text-indigo-600 dark:text-indigo-400">
                  ${finalTotal.toFixed(2)}
                </span>
              </div>
            </div>

            {/* Checkout Button */}
            <button
              onClick={() => navigate('/checkout', { state: { couponCode: couponDiscount?.code } })}
              className="w-full py-4 rounded-full bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white font-extrabold text-xs uppercase tracking-wider hover:from-indigo-500 hover:to-pink-500 transition-all shadow-xl shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-[1.02] active:scale-[0.98] flex items-center justify-center gap-2"
            >
              <span>{t('cart.checkout')}</span>
              <ArrowRight className="w-4 h-4" />
            </button>

            <div className="flex items-center justify-center gap-2 text-[11px] text-zinc-400 pt-2 font-medium">
              <ShieldCheck className="w-3.5 h-3.5 text-emerald-500" />
              <span>Encrypted 256-bit secure checkout</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
