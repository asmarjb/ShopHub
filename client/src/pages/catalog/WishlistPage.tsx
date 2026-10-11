import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Heart, ShoppingBag, ArrowRight } from 'lucide-react';
import { api } from '../../services/api';
import { Product } from '../../types';
import { ProductCard } from '../../components/product/ProductCard';
import { useWishlist } from '../../context/WishlistContext';

export const WishlistPage: React.FC = () => {
  const { t } = useTranslation();
  const { wishlistIds } = useWishlist();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchWishlistProducts = async () => {
      setLoading(true);
      try {
        const res = await api.get('/wishlist');
        const items = Array.isArray(res.data) ? res.data : (res.data?.data || []);
        const mappedProducts: Product[] = items.map((i: any) => ({
          id: i.productId || i.id,
          name: i.productName || i.name || 'Product',
          slug: i.productSlug || i.slug || (i.productId || i.id),
          sku: i.sku || '',
          basePrice: i.basePrice || 0,
          compareAtPrice: i.compareAtPrice,
          categoryName: i.categoryName || '',
          categoryId: i.categoryId || '',
          brandName: i.brandName || '',
          primaryImageUrl: i.primaryImageUrl,
          averageRating: i.averageRating || 5.0,
          reviewCount: i.reviewCount || 0,
          isFeatured: false,
          isActive: true,
          isDeleted: false,
          stockQuantity: 10,
        }));
        setProducts(mappedProducts);
      } catch {
        setProducts([]);
      } finally {
        setLoading(false);
      }
    };

    fetchWishlistProducts();
  }, [wishlistIds]);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-8">
      <div>
        <h1 className="text-3xl font-extrabold text-gray-900 dark:text-gray-100 flex items-center gap-3">
          <Heart className="w-8 h-8 text-rose-500 fill-rose-500" />
          {t('nav.wishlist')} ({products.length})
        </h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
          All your saved and favorite items in one place.
        </p>
      </div>

      {loading ? (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-6 animate-pulse">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="h-72 bg-gray-200 dark:bg-gray-800 rounded-2xl" />
          ))}
        </div>
      ) : products.length > 0 ? (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-6">
          {products.map((prod) => (
            <ProductCard key={prod.id} product={prod} />
          ))}
        </div>
      ) : (
        <div className="text-center py-20 bg-white dark:bg-gray-800 rounded-3xl border border-gray-200 dark:border-gray-700 p-8 space-y-4">
          <Heart className="w-16 h-16 text-gray-300 dark:text-gray-600 mx-auto" />
          <h2 className="text-lg font-bold text-gray-800 dark:text-gray-200">
            Your wishlist is empty
          </h2>
          <p className="text-xs text-gray-500 max-w-sm mx-auto">
            Click on the heart icon on any product to save items for later.
          </p>
          <div className="pt-2">
            <Link
              to="/products"
              className="inline-flex items-center gap-2 px-6 py-2.5 rounded-xl bg-indigo-600 text-white font-bold text-xs hover:bg-indigo-700 transition"
            >
              Explore Products <ArrowRight className="w-4 h-4" />
            </Link>
          </div>
        </div>
      )}
    </div>
  );
};
