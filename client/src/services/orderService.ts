import { api } from './api';
import { ApiResponse, PagedResult } from '../types';

export interface ShippingMethod {
  id: string;
  name: string;
  description?: string;
  price: number;
  estimatedDeliveryDays: number;
  isActive: boolean;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  variantId?: string;
  variantDescription?: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  primaryImageUrl?: string;
}

export interface OrderStatusHistory {
  id: string;
  status: string;
  comment?: string;
  createdAt: string;
}

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  status: string;
  subTotal: number;
  discountAmount: number;
  shippingAmount: number;
  totalAmount: number;
  shippingAddressLine1: string;
  shippingAddressLine2?: string;
  shippingCity: string;
  shippingState: string;
  shippingPostalCode: string;
  shippingCountry: string;
  shippingMethodId: string;
  shippingMethodName: string;
  paymentMethod: string;
  couponCode?: string;
  customerNote?: string;
  createdAt: string;
  items: OrderItem[];
  statusHistory: OrderStatusHistory[];
}

export interface CheckoutRequest {
  shippingFullName: string;
  shippingPhoneNumber: string;
  shippingAddressLine1: string;
  shippingAddressLine2?: string;
  shippingCity: string;
  shippingState: string;
  shippingPostalCode: string;
  shippingCountry: string;
  shippingMethodId: string;
  couponCode?: string;
  paymentProvider?: number;
  notes?: string;
}

export interface ReturnRequest {
  id: string;
  orderId: string;
  orderNumber: string;
  status: string;
  reason: string;
  adminNotes?: string;
  createdAt: string;
}

export const orderService = {
  getShippingMethods: async (): Promise<ShippingMethod[]> => {
    const res = await api.get<any>('/shipping-methods');
    return (res.data as any)?.data ?? res.data ?? [];
  },

  checkout: async (data: CheckoutRequest): Promise<Order> => {
    const res = await api.post<any>('/orders/checkout', data);
    return (res.data as any)?.data ?? res.data;
  },

  getMyOrders: async (pageNumber = 1, pageSize = 10): Promise<PagedResult<Order>> => {
    const res = await api.get<any>(`/orders/my-orders?pageNumber=${pageNumber}&pageSize=${pageSize}`);
    return (res.data as any)?.data ?? res.data;
  },

  getOrderById: async (id: string): Promise<Order> => {
    const res = await api.get<any>(`/orders/${id}`);
    return (res.data as any)?.data ?? res.data;
  },

  trackOrderByNumber: async (orderNumber: string): Promise<Order> => {
    const res = await api.get<any>(`/orders/track/${encodeURIComponent(orderNumber)}`);
    return (res.data as any)?.data ?? res.data;
  },

  cancelOrder: async (id: string, reason?: string): Promise<void> => {
    await api.post(`/orders/${id}/cancel`, { reason });
  },

  createReturnRequest: async (data: { orderId: string; orderItemId?: string; reason: string }): Promise<void> => {
    await api.post('/returns', data);
  },

  getMyReturns: async (): Promise<ReturnRequest[]> => {
    const res = await api.get<any>('/returns/my-returns');
    return (res.data as any)?.data ?? res.data ?? [];
  },
};
