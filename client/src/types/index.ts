export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  roles: string[];
  permissions: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface ProductImage {
  id: string;
  imageUrl: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ProductVariant {
  id: string;
  sku: string;
  name?: string;
  price: number;
  originalPrice?: number;
  stockQuantity: number;
  attributes: Record<string, string>;
}

export interface Product {
  id: string;
  name: string;
  slug: string;
  shortDescription?: string;
  description?: string;
  fullDescription?: string;
  basePrice: number;
  discountPrice?: number;
  sku: string;
  stockQuantity: number;
  categoryId: string;
  categoryName?: string;
  brandId?: string;
  brandName?: string;
  isFeatured: boolean;
  averageRating: number;
  reviewCount: number;
  primaryImageUrl?: string;
  images: ProductImage[];
  variants: ProductVariant[];
  tags: string[];
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  parentId?: string;
  parentCategoryId?: string;
  children?: Category[];
  productCount?: number;
}

export interface Brand {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
}

export interface Banner {
  id: string;
  title: string;
  subTitle?: string;
  imageUrl: string;
  linkUrl?: string;
  displayOrder: number;
  isActive: boolean;
}

export interface Review {
  id: string;
  productId: string;
  userId: string;
  userName: string;
  rating: number;
  title?: string;
  comment: string;
  isVerifiedPurchase: boolean;
  createdAt: string;
}

export interface CartItem {
  id: string;
  productId?: string;
  productName: string;
  productSlug?: string;
  variantId?: string;
  variantName?: string;
  variantDescription?: string;
  primaryImageUrl?: string;
  imageUrl?: string;
  sku?: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface Cart {
  id: string;
  items: CartItem[];
  subTotal: number;
  discountAmount: number;
  finalTotal: number;
  couponCode?: string;
}
