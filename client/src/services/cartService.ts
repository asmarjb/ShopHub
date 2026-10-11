import { api } from './api';
import { ApiResponse, Cart } from '../types';

export const cartService = {
  getCart: async (): Promise<Cart> => {
    const res = await api.get<any>('/cart');
    return (res.data as any)?.data ?? res.data;
  },

  addItem: async (productId: string, variantId?: string, quantity = 1): Promise<Cart> => {
    const res = await api.post<any>('/cart/items', {
      productVariantId: variantId || undefined,
      variantId: variantId || undefined,
      productId: productId || undefined,
      quantity,
    });
    return (res.data as any)?.data ?? res.data;
  },

  updateQuantity: async (itemId: string, quantity: number): Promise<Cart> => {
    const res = await api.put<any>(`/cart/items/${itemId}`, {
      quantity,
    });
    return (res.data as any)?.data ?? res.data;
  },

  removeItem: async (itemId: string): Promise<Cart> => {
    const res = await api.delete<any>(`/cart/items/${itemId}`);
    return (res.data as any)?.data ?? res.data;
  },

  clearCart: async (): Promise<void> => {
    await api.delete('/cart/clear');
  },

  validateCoupon: async (code: string, subTotal: number): Promise<{
    code: string;
    couponType: string;
    discountAmount: number;
    finalTotal: number;
  }> => {
    const res = await api.post<any>('/coupons/validate', {
      code,
      subTotal,
    });
    return (res.data as any)?.data ?? res.data;
  },
};
