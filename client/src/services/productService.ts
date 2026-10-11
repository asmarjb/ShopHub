import { api } from './api';
import { ApiResponse, PagedResult, Product, Category, Brand, Banner, Review } from '../types';

export interface ProductFilterParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: string;
  brandId?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface AutocompleteProduct {
  id: string;
  name: string;
  slug: string;
  primaryImageUrl?: string;
  basePrice?: number;
  categoryName?: string;
}

export const productService = {
  getProducts: async (params: ProductFilterParams = {}): Promise<PagedResult<Product>> => {
    const res = await api.get<any>('/products', { params });
    return (res.data as any)?.data ?? res.data;
  },

  getProductBySlug: async (slug: string): Promise<Product> => {
    const res = await api.get<any>(`/products/slug/${slug}`);
    return (res.data as any)?.data ?? res.data;
  },

  getProductById: async (id: string): Promise<Product> => {
    const res = await api.get<any>(`/products/${id}`);
    return (res.data as any)?.data ?? res.data;
  },

  getRelatedProducts: async (productId: string, take = 4): Promise<Product[]> => {
    const res = await api.get<any>(`/products/${productId}/related?take=${take}`);
    return (res.data as any)?.data ?? res.data ?? [];
  },

  getAutocomplete: async (query: string): Promise<AutocompleteProduct[]> => {
    if (!query || query.length < 2) return [];
    const res = await api.get<any>(`/products/autocomplete?q=${encodeURIComponent(query)}`);
    return (res.data as any)?.data ?? res.data ?? [];
  },

  getCategories: async (): Promise<Category[]> => {
    const res = await api.get<any>('/categories');
    return (res.data as any)?.data ?? res.data ?? [];
  },

  getBrands: async (): Promise<Brand[]> => {
    const res = await api.get<any>('/brands');
    return (res.data as any)?.data ?? res.data ?? [];
  },

  getBanners: async (): Promise<Banner[]> => {
    const res = await api.get<any>('/banners');
    return (res.data as any)?.data ?? res.data ?? [];
  },

  getProductReviews: async (productId: string): Promise<Review[]> => {
    const res = await api.get<any>(`/reviews/product/${productId}`);
    return (res.data as any)?.data ?? res.data ?? [];
  },

  addReview: async (data: { productId: string; rating: number; title?: string; comment: string }): Promise<void> => {
    await api.post('/reviews', data);
  },
};
