import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Truck,
  CreditCard,
  MapPin,
  ShieldCheck,
  CheckCircle2,
  ChevronRight,
  Lock,
  AlertTriangle,
} from 'lucide-react';
import { useCart } from '../../context/CartContext';
import { useAuth } from '../../context/AuthContext';
import { orderService, ShippingMethod } from '../../services/orderService';
import toast from 'react-hot-toast';

export const CheckoutPage: React.FC = () => {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();
  const { cart, clearCart } = useCart();
  const { user, isAuthenticated } = useAuth();

  const [shippingMethods, setShippingMethods] = useState<ShippingMethod[]>([]);
  const [selectedShippingMethodId, setSelectedShippingMethodId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('CreditCard');
  const [customerNote, setCustomerNote] = useState('');

  // Form Fields
  const [fullName, setFullName] = useState(
    user ? `${user.firstName || ''} ${user.lastName || ''}`.trim() : 'Alex Morgan'
  );
  const [phoneNumber, setPhoneNumber] = useState(user?.phoneNumber || '+1 (555) 234-5678');
  const [addressLine1, setAddressLine1] = useState('742 Evergreen Terrace');
  const [addressLine2, setAddressLine2] = useState('Apt 4B');
  const [city, setCity] = useState('New York');
  const [state, setState] = useState('NY');
  const [postalCode, setPostalCode] = useState('10001');
  const [country, setCountry] = useState('United States');

  const [loading, setLoading] = useState(false);
  const couponCode = (location.state as any)?.couponCode || '';

  useEffect(() => {
    if (user) {
      const computedName = `${user.firstName || ''} ${user.lastName || ''}`.trim();
      if (computedName) setFullName(computedName);
      if (user.phoneNumber) setPhoneNumber(user.phoneNumber);
    }
  }, [user]);

  useEffect(() => {
    const fetchMethods = async () => {
      try {
        const methods = await orderService.getShippingMethods();
        setShippingMethods(methods.filter((m) => m.isActive));
        if (methods.length > 0) {
          setSelectedShippingMethodId(methods[0].id);
        }
      } catch {
        toast.error('Unable to load shipping methods.');
      }
    };

    fetchMethods();
  }, []);

  if (!cart || !cart.items || cart.items.length === 0) {
    return (
      <div className="max-w-xl mx-auto py-24 text-center space-y-4">
        <h2 className="text-2xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100">
          Your Shopping Bag is Empty
        </h2>
        <Link
          to="/products"
          className="px-6 py-2.5 rounded-full bg-black text-white dark:bg-white dark:text-black text-xs font-bold uppercase tracking-wider inline-block"
        >
          Return to Catalog
        </Link>
      </div>
    );
  }

  const selectedShipping = shippingMethods.find((m) => m.id === selectedShippingMethodId);
  const shippingCost = selectedShipping?.price || 0;
  const subTotal = cart.subTotal || 0;
  const finalTotal = subTotal + shippingCost - (cart.discountAmount || 0);

  const isAdmin = Boolean(user?.roles?.some((r) => ['Admin', 'SuperAdmin', 'Manager'].includes(r)));

  const handleSubmitOrder = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isAuthenticated) {
      toast.error('Please sign in to complete your order.');
      navigate('/login', { state: { from: '/checkout' } });
      return;
    }

    if (isAdmin) {
      toast.error('Administrators and managers cannot place orders. Please switch to a customer account.');
      return;
    }

    if (!fullName.trim()) {
      toast.error('Please enter your full recipient name.');
      return;
    }

    if (!phoneNumber.trim()) {
      toast.error('Please enter a valid phone number.');
      return;
    }

    if (!addressLine1.trim()) {
      toast.error('Please enter delivery address line 1.');
      return;
    }

    if (!selectedShippingMethodId) {
      toast.error('Please select a shipping delivery method.');
      return;
    }

    setLoading(true);
    try {
      const paymentProvider = paymentMethod === 'CashOnDelivery' ? 3 : 1; // 1: Mock (Card), 3: CashOnDelivery

      const order = await orderService.checkout({
        shippingFullName: fullName.trim(),
        shippingPhoneNumber: phoneNumber.trim(),
        shippingAddressLine1: addressLine1.trim(),
        shippingAddressLine2: addressLine2.trim() || undefined,
        shippingCity: city.trim(),
        shippingState: state.trim(),
        shippingPostalCode: postalCode.trim(),
        shippingCountry: country.trim(),
        shippingMethodId: selectedShippingMethodId,
        paymentProvider,
        couponCode: couponCode || undefined,
        notes: customerNote.trim() || undefined,
      });

      await clearCart();
      toast.success('Your order has been placed successfully!');
      navigate('/order-success', { state: { order } });
    } catch (err: any) {
      const errorData = err.response?.data;
      let errorMsg = errorData?.message || errorData?.title;
      if (errorData?.errors) {
        const firstKey = Object.keys(errorData.errors)[0];
        if (firstKey && errorData.errors[firstKey]?.length > 0) {
          errorMsg = errorData.errors[firstKey][0];
        }
      }
      toast.error(errorMsg || 'An error occurred while placing order.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-10">
      <div className="border-b border-zinc-200 dark:border-zinc-800 pb-5">
        <h1 className="text-3xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans">
          Checkout & Finalize
        </h1>
        <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1 font-light">
          Review shipping address, select delivery speed, and confirm payment details.
        </p>
      </div>

      {isAdmin && (
        <div className="p-5 rounded-3xl bg-amber-500/10 border-2 border-amber-500/30 text-amber-800 dark:text-amber-300 flex items-start sm:items-center gap-4 shadow-sm">
          <div className="w-10 h-10 rounded-2xl bg-amber-500/20 flex items-center justify-center flex-shrink-0 text-amber-600 dark:text-amber-400">
            <AlertTriangle className="w-5 h-5" />
          </div>
          <div className="text-xs space-y-1">
            <h4 className="font-extrabold uppercase tracking-wider text-amber-900 dark:text-amber-200">
              Admin Account Detected — Orders Restricted
            </h4>
            <p className="text-zinc-600 dark:text-zinc-400 font-medium">
              Administrators and store managers are not permitted to place orders. Please sign in with a registered customer account (e.g. <span className="font-mono font-bold text-amber-700 dark:text-amber-300">customer@test.com</span> / <span className="font-mono font-bold text-amber-700 dark:text-amber-300">Customer123!</span>).
            </p>
          </div>
        </div>
      )}

      <form onSubmit={handleSubmitOrder} className="grid grid-cols-1 lg:grid-cols-3 gap-10">
        {/* Left Columns: Address, Shipping & Payment */}
        <div className="lg:col-span-2 space-y-8">
          
          {/* 1. Shipping Address */}
          <div className="p-7 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 shadow-sm space-y-5">
            <h2 className="text-sm font-black uppercase tracking-wider text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
              <MapPin className="w-4 h-4 text-indigo-500" />
              1. Delivery Address & Recipient
            </h2>

            <div className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                <div>
                  <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                    Recipient Full Name *
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="e.g. Alex Morgan"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    className="w-full px-4 py-3 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                  />
                </div>

                <div>
                  <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                    Phone Number *
                  </label>
                  <input
                    type="tel"
                    required
                    placeholder="e.g. +1 (555) 234-5678"
                    value={phoneNumber}
                    onChange={(e) => setPhoneNumber(e.target.value)}
                    className="w-full px-4 py-3 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                  />
                </div>
              </div>
              <div>
                <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                  Street Address Line 1 *
                </label>
                <input
                  type="text"
                  required
                  placeholder="Street name, house / apartment number..."
                  value={addressLine1}
                  onChange={(e) => setAddressLine1(e.target.value)}
                  className="w-full px-4 py-3 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                />
              </div>

              <div>
                <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                  Apartment, Suite, Unit (Optional)
                </label>
                <input
                  type="text"
                  placeholder="Building, entrance code, floor..."
                  value={addressLine2}
                  onChange={(e) => setAddressLine2(e.target.value)}
                  className="w-full px-4 py-3 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                />
              </div>

              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                <div>
                  <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">City *</label>
                  <input
                    type="text"
                    required
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                    className="w-full px-3 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                  />
                </div>
                <div>
                  <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">State / Region *</label>
                  <input
                    type="text"
                    required
                    value={state}
                    onChange={(e) => setState(e.target.value)}
                    className="w-full px-3 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                  />
                </div>
                <div>
                  <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">Postal Code *</label>
                  <input
                    type="text"
                    required
                    value={postalCode}
                    onChange={(e) => setPostalCode(e.target.value)}
                    className="w-full px-3 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                  />
                </div>
                <div>
                  <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">Country *</label>
                  <input
                    type="text"
                    required
                    value={country}
                    onChange={(e) => setCountry(e.target.value)}
                    className="w-full px-3 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* 2. Shipping Method Selection */}
          <div className="p-7 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 shadow-sm space-y-5">
            <h2 className="text-sm font-black uppercase tracking-wider text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
              <Truck className="w-4 h-4" />
              2. Shipping Method
            </h2>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              {shippingMethods.map((method) => {
                const isSelected = selectedShippingMethodId === method.id;
                return (
                  <div
                    key={method.id}
                    onClick={() => setSelectedShippingMethodId(method.id)}
                    className={`p-5 rounded-2xl border-2 cursor-pointer transition ${
                      isSelected
                        ? 'border-indigo-600 dark:border-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/40 shadow-sm'
                        : 'border-zinc-200 dark:border-zinc-800 hover:border-indigo-300'
                    }`}
                  >
                    <div className="flex items-center justify-between mb-1.5">
                      <span className="font-extrabold text-xs uppercase tracking-wide text-zinc-900 dark:text-zinc-100">
                        {method.name}
                      </span>
                      <span className="font-black text-sm text-indigo-600 dark:text-indigo-400">
                        ${method.price.toFixed(2)}
                      </span>
                    </div>
                    {method.description && (
                      <p className="text-xs text-zinc-500 dark:text-zinc-400 font-light">{method.description}</p>
                    )}
                    <span className="text-[10px] text-zinc-400 block mt-2 font-mono">
                      Estimated: {method.estimatedDeliveryDays}
                    </span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* 3. Payment Method Selection (Mock) */}
          <div className="p-7 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 shadow-sm space-y-5">
            <h2 className="text-sm font-black uppercase tracking-wider text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
              <CreditCard className="w-4 h-4 text-indigo-500" />
              3. Payment Option (Encrypted)
            </h2>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              <div
                onClick={() => setPaymentMethod('CreditCard')}
                className={`p-5 rounded-2xl border-2 cursor-pointer flex items-center gap-3.5 transition ${
                  paymentMethod === 'CreditCard'
                    ? 'border-indigo-600 dark:border-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/40 shadow-sm'
                    : 'border-zinc-200 dark:border-zinc-800 hover:border-indigo-300'
                }`}
              >
                <CreditCard className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
                <div>
                  <h4 className="font-extrabold text-xs uppercase tracking-wider text-zinc-900 dark:text-zinc-100">Credit / Debit Card</h4>
                  <p className="text-[10px] text-zinc-400">Visa, Mastercard, Amex (Mock)</p>
                </div>
              </div>

              <div
                onClick={() => setPaymentMethod('CashOnDelivery')}
                className={`p-5 rounded-2xl border-2 cursor-pointer flex items-center gap-3.5 transition ${
                  paymentMethod === 'CashOnDelivery'
                    ? 'border-indigo-600 dark:border-indigo-500 bg-indigo-50/50 dark:bg-indigo-950/40 shadow-sm'
                    : 'border-zinc-200 dark:border-zinc-800 hover:border-indigo-300'
                }`}
              >
                <Truck className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                <div>
                  <h4 className="font-extrabold text-xs uppercase tracking-wider text-zinc-900 dark:text-zinc-100">Cash on Delivery</h4>
                  <p className="text-[10px] text-zinc-400">Pay upon parcel inspection</p>
                </div>
              </div>
            </div>

            <div>
              <label className="block text-[11px] font-black uppercase tracking-widest text-zinc-400 mb-1.5">
                Delivery Instructions or Notes (Optional)
              </label>
              <textarea
                rows={2}
                placeholder="e.g. Please leave package at front desk concierge..."
                value={customerNote}
                onChange={(e) => setCustomerNote(e.target.value)}
                className="w-full px-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
          </div>
        </div>

        {/* Right Column: Order Summary */}
        <div className="space-y-6">
          <div className="p-7 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 shadow-sm space-y-5">
            <h3 className="font-black text-sm uppercase tracking-wider text-zinc-900 dark:text-zinc-100 border-b border-zinc-100 dark:border-zinc-800 pb-3.5">
              Order Items ({cart.items.length})
            </h3>

            <div className="space-y-3.5 max-h-64 overflow-y-auto pr-2 divide-y divide-zinc-100 dark:divide-zinc-800">
              {cart.items.map((it) => (
                <div key={it.id} className="pt-2.5 flex items-center justify-between text-xs">
                  <div className="pr-3">
                    <p className="font-bold line-clamp-1 text-zinc-900 dark:text-zinc-100">{it.productName}</p>
                    <span className="text-[11px] text-zinc-400 font-mono">Qty: {it.quantity}</span>
                  </div>
                  <span className="font-black text-zinc-900 dark:text-zinc-100">${it.totalPrice.toFixed(2)}</span>
                </div>
              ))}
            </div>

            <div className="space-y-2.5 pt-4 border-t border-zinc-100 dark:border-zinc-800 text-xs">
              <div className="flex justify-between text-zinc-500 font-medium">
                <span>Items Subtotal:</span>
                <span className="font-bold text-zinc-900 dark:text-zinc-100">${subTotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-zinc-500 font-medium">
                <span>Shipping:</span>
                <span className="font-bold text-zinc-900 dark:text-zinc-100">${shippingCost.toFixed(2)}</span>
              </div>
              {cart.discountAmount > 0 && (
                <div className="flex justify-between text-emerald-600 dark:text-emerald-400 font-bold">
                  <span>Discount:</span>
                  <span>-${cart.discountAmount.toFixed(2)}</span>
                </div>
              )}
              <div className="flex justify-between text-base font-black text-zinc-900 dark:text-zinc-100 pt-3 border-t border-zinc-200 dark:border-zinc-800">
                <span className="uppercase tracking-wider">Total:</span>
                <span className="text-xl text-indigo-600 dark:text-indigo-400">${finalTotal.toFixed(2)}</span>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading || isAdmin}
              className={`w-full py-4 rounded-full font-extrabold text-xs uppercase tracking-wider transition-all flex items-center justify-center gap-2 ${
                isAdmin
                  ? 'bg-zinc-200 dark:bg-zinc-800 text-zinc-400 dark:text-zinc-500 cursor-not-allowed shadow-none'
                  : 'bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white hover:from-indigo-500 hover:to-pink-500 shadow-xl shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-[1.02] active:scale-[0.98]'
              } disabled:opacity-50`}
            >
              <Lock className="w-3.5 h-3.5" />
              <span>
                {loading
                  ? 'Processing Order...'
                  : isAdmin
                  ? 'Admins Cannot Place Orders'
                  : 'Authorize & Place Order'}
              </span>
            </button>
          </div>
        </div>
      </form>
    </div>
  );
};
