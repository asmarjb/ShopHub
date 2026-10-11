import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { ThemeProvider } from './context/ThemeContext';
import { AuthProvider } from './context/AuthContext';
import { WishlistProvider } from './context/WishlistContext';
import { CartProvider } from './context/CartContext';
import { Navbar } from './components/layout/Navbar';
import { Footer } from './components/layout/Footer';
import { ScrollToTop } from './components/common/ScrollToTop';

// Public Pages
import { Home } from './pages/Home';
import { ProductList } from './pages/catalog/ProductList';
import { ProductDetail } from './pages/catalog/ProductDetail';
import { CategoriesPage } from './pages/catalog/CategoriesPage';
import { WishlistPage } from './pages/catalog/WishlistPage';
import { CartPage } from './pages/checkout/CartPage';
import { CheckoutPage } from './pages/checkout/CheckoutPage';
import { OrderSuccessPage } from './pages/checkout/OrderSuccessPage';
import { MyOrdersPage } from './pages/account/MyOrdersPage';
import { OrderDetailPage } from './pages/account/OrderDetailPage';
import { OrderTrackingPage } from './pages/account/OrderTrackingPage';

// Auth Pages
import { Login } from './pages/auth/Login';
import { Register } from './pages/auth/Register';
import { ForgotPassword } from './pages/auth/ForgotPassword';
import { ResetPassword } from './pages/auth/ResetPassword';
import { ConfirmEmail } from './pages/auth/ConfirmEmail';

// Admin Components & Pages (Phase 8a & 8b)
import { AdminLayout } from './admin/components/AdminLayout';
import { PermissionGuard } from './admin/components/PermissionGuard';
import { Dashboard } from './admin/pages/Dashboard';
import { AdminProductList } from './admin/pages/products/AdminProductList';
import { AdminProductForm } from './admin/pages/products/AdminProductForm';
import { AdminCategoryList } from './admin/pages/categories/AdminCategoryList';
import { AdminBrandList } from './admin/pages/brands/AdminBrandList';
import { AdminDeletedItems } from './admin/pages/deleted/AdminDeletedItems';
import { AdminOrderList } from './admin/pages/orders/AdminOrderList';
import { AdminUserList } from './admin/pages/users/AdminUserList';
import { AdminRoleList } from './admin/pages/roles/AdminRoleList';
import { AdminCouponList } from './admin/pages/coupons/AdminCouponList';
import { AdminReviewList } from './admin/pages/reviews/AdminReviewList';
import { AdminBannerList } from './admin/pages/banners/AdminBannerList';
import { AdminShippingList } from './admin/pages/shipping/AdminShippingList';
import { AdminReturnList } from './admin/pages/returns/AdminReturnList';
import { AdminAuditLogs } from './admin/pages/audit/AdminAuditLogs';
import { AdminStockMovements } from './admin/pages/stock/AdminStockMovements';

export const App: React.FC = () => {
  return (
    <ThemeProvider>
      <AuthProvider>
        <WishlistProvider>
          <CartProvider>
            <BrowserRouter>
              <ScrollToTop />
              <Routes>
                {/* Admin Panel Routes (Protected by PermissionGuard) */}
                <Route
                  path="/admin"
                  element={
                    <PermissionGuard>
                      <AdminLayout />
                    </PermissionGuard>
                  }
                >
                  <Route index element={<Dashboard />} />
                  <Route path="products" element={<AdminProductList />} />
                  <Route path="products/new" element={<AdminProductForm />} />
                  <Route path="products/edit/:id" element={<AdminProductForm />} />
                  <Route path="categories" element={<AdminCategoryList />} />
                  <Route path="brands" element={<AdminBrandList />} />
                  <Route path="deleted" element={<AdminDeletedItems />} />
                  <Route path="orders" element={<AdminOrderList />} />
                  <Route path="users" element={<AdminUserList />} />
                  <Route path="roles" element={<AdminRoleList />} />
                  <Route path="coupons" element={<AdminCouponList />} />
                  <Route path="reviews" element={<AdminReviewList />} />
                  <Route path="banners" element={<AdminBannerList />} />
                  <Route path="shipping" element={<AdminShippingList />} />
                  <Route path="returns" element={<AdminReturnList />} />
                  <Route path="audit" element={<AdminAuditLogs />} />
                  <Route path="stock" element={<AdminStockMovements />} />
                </Route>

                {/* Public Storefront Routes */}
                <Route
                  path="*"
                  element={
                    <div className="min-h-screen flex flex-col bg-gray-50 dark:bg-gray-900 text-gray-900 dark:text-gray-100 transition-colors duration-200">
                      <Navbar />
                      <main className="flex-1">
                        <Routes>
                          <Route path="/" element={<Home />} />
                          <Route path="/products" element={<ProductList />} />
                          <Route path="/products/:slug" element={<ProductDetail />} />
                          <Route path="/categories" element={<CategoriesPage />} />
                          <Route path="/wishlist" element={<WishlistPage />} />
                          <Route path="/cart" element={<CartPage />} />
                          <Route path="/checkout" element={<CheckoutPage />} />
                          <Route path="/order-success" element={<OrderSuccessPage />} />
                          <Route path="/my-orders" element={<MyOrdersPage />} />
                          <Route path="/my-orders/:id" element={<OrderDetailPage />} />
                          <Route path="/track-order" element={<OrderTrackingPage />} />

                          {/* Auth */}
                          <Route path="/login" element={<Login />} />
                          <Route path="/register" element={<Register />} />
                          <Route path="/forgot-password" element={<ForgotPassword />} />
                          <Route path="/reset-password" element={<ResetPassword />} />
                          <Route path="/confirm-email" element={<ConfirmEmail />} />

                          {/* Fallback */}
                          <Route path="*" element={<Navigate to="/" replace />} />
                        </Routes>
                      </main>
                      <Footer />
                    </div>
                  }
                />
              </Routes>
              <Toaster position="top-right" />
            </BrowserRouter>
          </CartProvider>
        </WishlistProvider>
      </AuthProvider>
    </ThemeProvider>
  );
};

export default App;
