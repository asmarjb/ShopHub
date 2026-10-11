import React from 'react';
import { Link } from 'react-router-dom';
import { Heart, Star, ShoppingBag } from 'lucide-react';
import { Product } from '../../types';
import { useWishlist } from '../../context/WishlistContext';

interface ProductCardProps {
  product: Product;
}

import { getProductImage } from '../../constants/productImages';

export const ProductCard: React.FC<ProductCardProps> = ({ product }) => {
  const { isInWishlist, toggleWishlist } = useWishlist();
  const wishlisted = isInWishlist(product.id);

  const price = product.discountPrice ?? product.basePrice;
  const hasDiscount = !!product.discountPrice && product.discountPrice < product.basePrice;

  const imageSrc = getProductImage(product.slug, product.primaryImageUrl, product.categoryName);

  return (
    <div className="group relative bg-white dark:bg-zinc-900 rounded-2xl border border-zinc-200 dark:border-zinc-800 overflow-hidden shadow-sm hover:shadow-xl hover:shadow-indigo-500/5 hover:border-indigo-300 dark:hover:border-indigo-700/60 transition-all duration-300 flex flex-col">
      {/* Product Image & Badges */}
      <div className="relative aspect-square overflow-hidden bg-zinc-100 dark:bg-zinc-800/60">
        <Link to={`/products/${product.slug || product.id}`}>
          <img
            src={imageSrc}
            alt={product.name}
            onError={(e) => {
              e.currentTarget.src = getProductImage(product.slug, undefined, product.categoryName);
            }}
            className="w-full h-full object-cover object-center group-hover:scale-105 transition-transform duration-500"
            loading="lazy"
          />
        </Link>

        {/* Colorful Badges */}
        <div className="absolute top-3 left-3 flex flex-col gap-1.5 pointer-events-none">
          {product.isFeatured && (
            <span className="px-2.5 py-1 text-[10px] font-black uppercase tracking-wider rounded-lg bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/30">
              Featured
            </span>
          )}
          {hasDiscount && (
            <span className="px-2.5 py-1 text-[10px] font-black uppercase tracking-wider rounded-lg bg-gradient-to-r from-rose-500 to-pink-500 text-white shadow-md shadow-rose-500/30">
              Sale
            </span>
          )}
        </div>

        {/* Wishlist Button */}
        <button
          onClick={() => toggleWishlist(product.id)}
          className="absolute top-3 right-3 p-2 rounded-full bg-white/90 dark:bg-zinc-900/90 text-zinc-600 dark:text-zinc-300 hover:text-rose-500 dark:hover:text-rose-400 shadow-md backdrop-blur-sm transition-transform active:scale-90"
        >
          <Heart className={`w-4 h-4 transition-colors ${wishlisted ? 'fill-rose-500 text-rose-500' : ''}`} />
        </button>
      </div>

      {/* Product Info */}
      <div className="p-4 sm:p-5 flex flex-col flex-1">
        {/* Category & Rating */}
        <div className="flex items-center justify-between text-xs text-zinc-500 dark:text-zinc-400 mb-2">
          <span className="truncate max-w-[140px] font-bold tracking-wide uppercase text-[11px] text-indigo-600 dark:text-indigo-400">
            {product.categoryName || 'Catalog'}
          </span>
          <div className="flex items-center gap-1">
            <Star className="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
            <span className="font-bold text-xs text-zinc-800 dark:text-zinc-200">
              {product.averageRating > 0 ? product.averageRating.toFixed(1) : '5.0'}
            </span>
          </div>
        </div>

        {/* Title */}
        <Link to={`/products/${product.slug || product.id}`}>
          <h3 className="font-bold text-sm text-zinc-900 dark:text-zinc-100 hover:text-indigo-600 dark:hover:text-indigo-400 line-clamp-2 transition-colors mb-2.5">
            {product.name}
          </h3>
        </Link>

        {/* Price & Action */}
        <div className="mt-auto pt-3 flex items-center justify-between border-t border-zinc-100 dark:border-zinc-800/80">
          <div>
            <div className="flex items-baseline gap-2">
              <span className="text-base font-extrabold text-indigo-600 dark:text-indigo-400">
                ${price.toFixed(2)}
              </span>
              {hasDiscount && (
                <span className="text-xs text-rose-500/80 line-through">
                  ${product.basePrice.toFixed(2)}
                </span>
              )}
            </div>
          </div>

          <Link
            to={`/products/${product.slug || product.id}`}
            className="p-2.5 rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 text-white hover:from-indigo-500 hover:to-violet-500 shadow-md shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-105 active:scale-95 transition-all"
            title="View Product"
          >
            <ShoppingBag className="w-4 h-4" />
          </Link>
        </div>
      </div>
    </div>
  );
};
