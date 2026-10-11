import { api } from '../../services/api';
import { ApiResponse, PagedResult, Product, Category, Brand, Banner, Review } from '../../types';
import { Order, ShippingMethod, ReturnRequest } from '../../services/orderService';

export interface DashboardStats {
  totalSales: number;
  totalOrders: number;
  totalCustomers: number;
  lowStockCount: number;
  ordersByStatus: Record<string, number>;
  revenueTrend: Array<{ date: string; amount: number }>;
  topProducts: Array<{ id: string; name: string; salesCount: number; revenue: number }>;
  lowStockProducts: Array<{ id: string; name: string; sku: string; stockQuantity: number }>;
}

export interface UpsertProductDto {
  name: string;
  slug?: string;
  shortDescription?: string;
  description?: string;
  basePrice: number;
  discountPrice?: number;
  sku: string;
  stockQuantity: number;
  categoryId: string;
  brandId?: string;
  isFeatured?: boolean;
  imageUrls?: string[];
  primaryImageUrl?: string;
  tags?: string[];
  variants?: Array<{
    sku: string;
    price: number;
    stockQuantity: number;
    attributes: Record<string, string>;
  }>;
}

export interface AdminUserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  emailConfirmed: boolean;
  roles: string[];
  createdAt: string;
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
}

export interface CouponDto {
  id: string;
  code: string;
  discountType: 'Percentage' | 'FixedAmount';
  discountValue: number;
  minOrderAmount?: number;
  maxDiscountAmount?: number;
  startDate: string;
  endDate: string;
  usageLimit?: number;
  usedCount: number;
  isActive: boolean;
}

export interface AuditLogDto {
  id: string;
  userId?: string;
  userName?: string;
  action: string;
  entityName: string;
  entityId?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  createdAt: string;
}

export interface StockMovementDto {
  id: string;
  productId: string;
  productName: string;
  variantId?: string;
  quantityChange: number;
  type: string;
  comment?: string;
  createdAt: string;
}

export const adminService = {
  // 1. Dashboard & Statistika
  getDashboardStats: async (): Promise<DashboardStats> => {
    const res = await api.get<ApiResponse<DashboardStats>>('/admin/statistics/dashboard');
    return res.data.data;
  },

  // 2. Məhsullar
  getProducts: async (params: any = {}): Promise<PagedResult<Product>> => {
    const res = await api.get<ApiResponse<PagedResult<Product>>>('/admin/products', { params });
    return res.data.data;
  },

  getProductById: async (id: string): Promise<Product> => {
    const res = await api.get<ApiResponse<Product>>(`/admin/products/${id}`);
    return res.data.data;
  },

  createProduct: async (data: UpsertProductDto): Promise<Product> => {
    const res = await api.post<ApiResponse<Product>>('/admin/products', data);
    return res.data.data;
  },

  updateProduct: async (id: string, data: UpsertProductDto): Promise<Product> => {
    const res = await api.put<ApiResponse<Product>>(`/admin/products/${id}`, data);
    return res.data.data;
  },

  deleteProduct: async (id: string): Promise<void> => {
    await api.delete(`/admin/products/${id}`);
  },

  exportProductsCsv: async (): Promise<Blob> => {
    const res = await api.get('/admin/products/export-csv', { responseType: 'blob' });
    return res.data;
  },

  // 3. Kateqoriyalar
  getCategories: async (): Promise<Category[]> => {
    const res = await api.get<ApiResponse<Category[]>>('/admin/categories');
    return res.data.data;
  },

  createCategory: async (data: { name: string; slug?: string; description?: string; parentId?: string }): Promise<Category> => {
    const res = await api.post<ApiResponse<Category>>('/admin/categories', data);
    return res.data.data;
  },

  updateCategory: async (id: string, data: { name: string; slug?: string; description?: string; parentId?: string }): Promise<Category> => {
    const res = await api.put<ApiResponse<Category>>(`/admin/categories/${id}`, data);
    return res.data.data;
  },

  deleteCategory: async (id: string, reassignToCategoryId?: string): Promise<void> => {
    const url = reassignToCategoryId 
      ? `/admin/categories/${id}?reassignToCategoryId=${encodeURIComponent(reassignToCategoryId)}`
      : `/admin/categories/${id}`;
    await api.delete(url);
  },

  // 4. Brendlər
  getBrands: async (): Promise<Brand[]> => {
    const res = await api.get<ApiResponse<Brand[]>>('/admin/brands');
    return res.data.data;
  },

  createBrand: async (data: { name: string; slug?: string; logoUrl?: string }): Promise<Brand> => {
    const res = await api.post<ApiResponse<Brand>>('/admin/brands', data);
    return res.data.data;
  },

  updateBrand: async (id: string, data: { name: string; slug?: string; logoUrl?: string }): Promise<Brand> => {
    const res = await api.put<ApiResponse<Brand>>(`/admin/brands/${id}`, data);
    return res.data.data;
  },

  deleteBrand: async (id: string): Promise<void> => {
    await api.delete(`/admin/brands/${id}`);
  },

  // 5. Silinmiş Elementlər (Soft-deleted) & Bərpa / Hard Delete
  getDeletedProducts: async (): Promise<Product[]> => {
    const res = await api.get<ApiResponse<Product[]>>('/admin/products/deleted');
    return res.data.data;
  },

  restoreProduct: async (id: string): Promise<void> => {
    await api.post(`/admin/products/${id}/restore`);
  },

  hardDeleteProduct: async (id: string): Promise<void> => {
    await api.delete(`/admin/products/${id}/hard-delete?confirm=true`);
  },

  getDeletedCategories: async (): Promise<Category[]> => {
    const res = await api.get<ApiResponse<Category[]>>('/admin/categories/deleted');
    return res.data.data;
  },

  restoreCategory: async (id: string): Promise<void> => {
    await api.post(`/admin/categories/${id}/restore`);
  },

  hardDeleteCategory: async (id: string): Promise<void> => {
    await api.delete(`/admin/categories/${id}/hard-delete?confirm=true`);
  },

  // 6. Sifarişlər (Admin)
  getOrders: async (params: any = {}): Promise<PagedResult<Order>> => {
    const res = await api.get<ApiResponse<PagedResult<Order>>>('/admin/orders', { params });
    return res.data.data;
  },

  updateOrderStatus: async (id: string, status: string, comment?: string): Promise<void> => {
    await api.put(`/admin/orders/${id}/status`, { status, comment });
  },

  exportOrdersCsv: async (): Promise<Blob> => {
    const res = await api.get('/admin/orders/export-csv', { responseType: 'blob' });
    return res.data;
  },

  // 7. İstifadəçilər
  getUsers: async (params: any = {}): Promise<PagedResult<AdminUserDto>> => {
    const res = await api.get<ApiResponse<PagedResult<AdminUserDto>>>('/admin/users', { params });
    return res.data.data;
  },

  assignUserRoles: async (userId: string, roles: string[]): Promise<void> => {
    await api.put(`/admin/users/${userId}/roles`, { roles });
  },

  // 8. Rollar və İcazələr
  getRoles: async (): Promise<RoleDto[]> => {
    const res = await api.get<ApiResponse<RoleDto[]>>('/admin/roles');
    return res.data.data;
  },

  getAllPermissions: async (): Promise<string[]> => {
    const res = await api.get<ApiResponse<string[]>>('/admin/roles/permissions');
    return res.data.data;
  },

  createRole: async (data: { name: string; description?: string; permissions: string[] }): Promise<RoleDto> => {
    const res = await api.post<ApiResponse<RoleDto>>('/admin/roles', data);
    return res.data.data;
  },

  updateRole: async (id: string, data: { name: string; description?: string; permissions: string[] }): Promise<RoleDto> => {
    const res = await api.put<ApiResponse<RoleDto>>(`/admin/roles/${id}`, data);
    return res.data.data;
  },

  deleteRole: async (id: string): Promise<void> => {
    await api.delete(`/admin/roles/${id}`);
  },

  // 9. Kuponlar (Admin)
  getCoupons: async (): Promise<CouponDto[]> => {
    const res = await api.get<ApiResponse<CouponDto[]>>('/admin/coupons');
    return res.data.data;
  },

  createCoupon: async (data: any): Promise<CouponDto> => {
    const res = await api.post<ApiResponse<CouponDto>>('/admin/coupons', data);
    return res.data.data;
  },

  updateCoupon: async (id: string, data: any): Promise<CouponDto> => {
    const res = await api.put<ApiResponse<CouponDto>>(`/admin/coupons/${id}`, data);
    return res.data.data;
  },

  deleteCoupon: async (id: string): Promise<void> => {
    await api.delete(`/admin/coupons/${id}`);
  },

  // 10. Rəylərin Moderasiyası
  getReviews: async (params: any = {}): Promise<PagedResult<Review & { isApproved: boolean }>> => {
    const res = await api.get<ApiResponse<PagedResult<any>>>('/admin/reviews', { params });
    return res.data.data;
  },

  moderateReview: async (id: string, isApproved: boolean): Promise<void> => {
    await api.put(`/admin/reviews/${id}/moderate`, { isApproved });
  },

  deleteReview: async (id: string): Promise<void> => {
    await api.delete(`/admin/reviews/${id}`);
  },

  // 11. Bannerlər (Admin)
  getBanners: async (): Promise<Banner[]> => {
    const res = await api.get<ApiResponse<Banner[]>>('/admin/banners');
    return res.data.data;
  },

  createBanner: async (data: any): Promise<Banner> => {
    const res = await api.post<ApiResponse<Banner>>('/admin/banners', data);
    return res.data.data;
  },

  updateBanner: async (id: string, data: any): Promise<Banner> => {
    const res = await api.put<ApiResponse<Banner>>(`/admin/banners/${id}`, data);
    return res.data.data;
  },

  deleteBanner: async (id: string): Promise<void> => {
    await api.delete(`/admin/banners/${id}`);
  },

  // 12. Çatdırılma Metodları (Admin)
  getShippingMethods: async (): Promise<ShippingMethod[]> => {
    const res = await api.get<ApiResponse<ShippingMethod[]>>('/admin/shipping-methods');
    return res.data.data;
  },

  createShippingMethod: async (data: any): Promise<ShippingMethod> => {
    const res = await api.post<ApiResponse<ShippingMethod>>('/admin/shipping-methods', data);
    return res.data.data;
  },

  updateShippingMethod: async (id: string, data: any): Promise<ShippingMethod> => {
    const res = await api.put<ApiResponse<ShippingMethod>>(`/admin/shipping-methods/${id}`, data);
    return res.data.data;
  },

  deleteShippingMethod: async (id: string): Promise<void> => {
    await api.delete(`/admin/shipping-methods/${id}`);
  },

  // 13. Geri Qaytarma Sorğuları
  getReturns: async (params: any = {}): Promise<PagedResult<ReturnRequest>> => {
    const res = await api.get<ApiResponse<PagedResult<ReturnRequest>>>('/admin/returns', { params });
    return res.data.data;
  },

  processReturn: async (id: string, status: string, adminNotes?: string): Promise<void> => {
    await api.put(`/admin/returns/${id}/process`, { status, adminNotes });
  },

  // 14. Audit Log Jurnalı
  getAuditLogs: async (params: any = {}): Promise<PagedResult<AuditLogDto>> => {
    const res = await api.get<ApiResponse<PagedResult<AuditLogDto>>>('/admin/audit-logs', { params });
    return res.data.data;
  },

  // 15. Stok Hərəkətləri
  getStockMovements: async (params: any = {}): Promise<PagedResult<StockMovementDto>> => {
    const res = await api.get<ApiResponse<PagedResult<StockMovementDto>>>('/admin/stock-movements', { params });
    return res.data.data;
  },

  adjustStock: async (data: { productId: string; variantId?: string; quantityChange: number; comment?: string }): Promise<void> => {
    await api.post('/admin/stock-movements/adjust', data);
  },
};
