import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Star,
  Heart,
  ShoppingBag,
  Truck,
  ShieldCheck,
  Check,
  ChevronRight,
  Clock,
  Sparkles,
} from 'lucide-react';
import { productService } from '../../services/productService';
import { Product, ProductVariant, Review } from '../../types';
import { useWishlist } from '../../context/WishlistContext';
import { useRecentlyViewed } from '../../hooks/useRecentlyViewed';
import { ProductCard } from '../../components/product/ProductCard';
import { getProductImage } from '../../constants/productImages';
import { useCart } from '../../context/CartContext';
import { api } from '../../services/api';
import toast from 'react-hot-toast';

export const ProductDetail: React.FC = () => {
  const { slug } = useParams<{ slug: string }>();
  const { t } = useTranslation();
  const { isInWishlist, toggleWishlist } = useWishlist();
  const { addToCart } = useCart();
  const { items: recentlyViewed, addProduct } = useRecentlyViewed();

  const [product, setProduct] = useState<Product | null>(null);
  const [selectedVariant, setSelectedVariant] = useState<ProductVariant | null>(null);
  const [selectedImage, setSelectedImage] = useState<string>('');
  const [quantity, setQuantity] = useState(1);
  const [relatedProducts, setRelatedProducts] = useState<Product[]>([]);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);

  // New review form state
  const [newRating, setNewRating] = useState(5);
  const [newTitle, setNewTitle] = useState('');
  const [newComment, setNewComment] = useState('');
  const [submittingReview, setSubmittingReview] = useState(false);

  useEffect(() => {
    const fetchProduct = async () => {
      if (!slug) return;
      setLoading(true);
      try {
        let prod: Product;
        try {
          prod = await productService.getProductBySlug(slug);
        } catch {
          prod = await productService.getProductById(slug);
        }

        setProduct(prod);
        const resolvedMain = getProductImage(prod.slug, prod.primaryImageUrl || prod.images?.[0]?.imageUrl, prod.categoryName);
        setSelectedImage(resolvedMain);

        if (prod.variants && prod.variants.length > 0) {
          setSelectedVariant(prod.variants[0]);
        } else {
          setSelectedVariant(null);
        }

        addProduct(prod);

        // Fetch related products and reviews in parallel
        const [related, revs] = await Promise.all([
          productService.getRelatedProducts(prod.id).catch(() => []),
          productService.getProductReviews(prod.id).catch(() => []),
        ]);
        setRelatedProducts(related);
        setReviews(revs);
      } catch (err) {
        toast.error('Failed to load product details.');
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [slug]);

  const handleAddToCart = async () => {
    if (!product) return;
    const targetVariantId = selectedVariant?.id || product.variants?.[0]?.id;
    await addToCart(product.id, targetVariantId, quantity);
  };

  const handleReviewSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!product || !newComment.trim()) return;

    setSubmittingReview(true);
    try {
      await productService.addReview({
        productId: product.id,
        rating: newRating,
        title: newTitle.trim() || undefined,
        comment: newComment.trim(),
      });
      toast.success('Thank you! Your review has been submitted for moderation.');
      setNewTitle('');
      setNewComment('');
    } catch {
      toast.error('Failed to submit review.');
    } finally {
      setSubmittingReview(false);
    }
  };

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 animate-pulse space-y-8">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-12">
          <div className="aspect-square bg-zinc-100 dark:bg-zinc-800 rounded-3xl" />
          <div className="space-y-4">
            <div className="h-8 bg-zinc-100 dark:bg-zinc-800 rounded w-3/4" />
            <div className="h-6 bg-zinc-100 dark:bg-zinc-800 rounded w-1/4" />
            <div className="h-24 bg-zinc-100 dark:bg-zinc-800 rounded" />
          </div>
        </div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="text-center py-20 space-y-4">
        <h2 className="text-2xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100">
          Product Not Found
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

  const currentPrice = selectedVariant
    ? selectedVariant.price
    : product.discountPrice ?? product.basePrice;
  const inStock = selectedVariant ? selectedVariant.stockQuantity > 0 : product.stockQuantity > 0;
  const wishlisted = isInWishlist(product.id);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-16">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-xs font-medium text-zinc-400">
        <Link to="/" className="hover:text-black dark:hover:text-white transition-colors">Home</Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <Link to="/products" className="hover:text-black dark:hover:text-white transition-colors">Products</Link>
        <ChevronRight className="w-3.5 h-3.5" />
        <span className="text-zinc-900 dark:text-zinc-100 font-semibold truncate max-w-xs">{product.name}</span>
      </nav>

      {/* Main Product Info */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-16">
        {/* Images Area */}
        <div className="space-y-4">
          <div className="aspect-square bg-zinc-100 dark:bg-zinc-900 rounded-3xl overflow-hidden border border-zinc-200 dark:border-zinc-800 shadow-sm relative">
            <img
              src={selectedImage || getProductImage(product.slug, undefined, product.categoryName)}
              alt={product.name}
              onError={(e) => {
                e.currentTarget.src = getProductImage(product.slug, undefined, product.categoryName);
              }}
              className="w-full h-full object-cover object-center"
            />
            <button
              onClick={() => toggleWishlist(product.id)}
              className="absolute top-4 right-4 p-3 rounded-full bg-white/90 dark:bg-zinc-900/90 text-zinc-600 dark:text-zinc-200 hover:text-rose-500 dark:hover:text-rose-400 shadow-md backdrop-blur-sm transition-transform active:scale-95"
            >
              <Heart className={`w-5 h-5 transition-colors ${wishlisted ? 'fill-rose-500 text-rose-500' : ''}`} />
            </button>
          </div>

          {/* Thumbnails */}
          {product.images && product.images.length > 1 && (
            <div className="flex gap-3 overflow-x-auto pb-2">
              {product.images.map((img) => (
                <button
                  key={img.id}
                  onClick={() => setSelectedImage(img.imageUrl)}
                  className={`w-20 h-20 rounded-2xl overflow-hidden border-2 flex-shrink-0 transition ${
                    selectedImage === img.imageUrl
                      ? 'border-indigo-600 dark:border-indigo-400 shadow-md shadow-indigo-500/20'
                      : 'border-transparent opacity-60 hover:opacity-100'
                  }`}
                >
                  <img src={img.imageUrl} alt="" className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Product Details & Actions */}
        <div className="space-y-7">
          <div>
            <div className="flex items-center gap-2 mb-3">
              {product.brandName && (
                <span className="text-[10px] font-black uppercase tracking-widest text-white bg-gradient-to-r from-indigo-600 to-purple-600 px-3 py-1 rounded-full shadow-sm shadow-indigo-500/25">
                  {product.brandName}
                </span>
              )}
              <span className="text-xs text-zinc-400">
                {t('products.sku')}: {selectedVariant?.sku || product.sku}
              </span>
            </div>

            <h1 className="text-3xl sm:text-4xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans leading-tight">
              {product.name}
            </h1>

            {/* Rating */}
            <div className="flex items-center gap-3 mt-3">
              <div className="flex items-center gap-1">
                <Star className="w-4 h-4 fill-amber-400 text-amber-400" />
                <span className="font-bold text-sm text-zinc-900 dark:text-zinc-100">
                  {product.averageRating > 0 ? product.averageRating.toFixed(1) : '5.0'}
                </span>
              </div>
              <span className="text-zinc-300 dark:text-zinc-700">|</span>
              <span className="text-xs text-zinc-500 dark:text-zinc-400 font-medium">
                {product.reviewCount} {t('products.reviews')}
              </span>
            </div>
          </div>

          {/* Price */}
          <div className="flex items-baseline gap-3 pb-6 border-b border-zinc-200 dark:border-zinc-800">
            <span className="text-3xl sm:text-4xl font-black text-indigo-600 dark:text-indigo-400">
              ${currentPrice.toFixed(2)}
            </span>
            {product.discountPrice && (
              <span className="text-lg text-rose-500/80 line-through">
                ${product.basePrice.toFixed(2)}
              </span>
            )}
          </div>

          {/* Short Description */}
          {product.shortDescription && (
            <p className="text-xs sm:text-sm text-zinc-600 dark:text-zinc-300 leading-relaxed font-light">
              {product.shortDescription}
            </p>
          )}

          {/* Variant Selector */}
          {product.variants && product.variants.length > 0 && (
            <div className="space-y-3">
              <label className="text-[11px] font-black uppercase tracking-widest text-indigo-600 dark:text-indigo-400">
                {t('products.selectVariant')}
              </label>
              <div className="flex flex-wrap gap-2.5">
                {product.variants.map((v) => {
                  const isSelected = selectedVariant?.id === v.id;
                  const label = v.name || Object.values(v.attributes || {}).join(' / ') || v.sku;
                  return (
                    <button
                      key={v.id}
                      onClick={() => setSelectedVariant(v)}
                      className={`px-4 py-2.5 text-xs font-bold rounded-xl border transition ${
                        isSelected
                          ? 'border-indigo-600 bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/25'
                          : 'border-zinc-200 dark:border-zinc-800 hover:border-indigo-400 text-zinc-700 dark:text-zinc-300'
                      }`}
                    >
                      {label} - ${v.price.toFixed(2)}
                    </button>
                  );
                })}
              </div>
            </div>
          )}

          {/* Stock Status & Quantity Picker */}
          <div className="space-y-4 pt-2">
            <div className="flex items-center gap-2 text-xs font-medium">
              {inStock ? (
                <span className="text-emerald-700 dark:text-emerald-400 font-bold bg-emerald-50 dark:bg-emerald-950/40 px-3 py-1 rounded-full border border-emerald-200 dark:border-emerald-800 flex items-center gap-1.5 shadow-sm">
                  <Check className="w-4 h-4 text-emerald-500" /> {t('products.inStock')}
                </span>
              ) : (
                <span className="text-rose-700 dark:text-rose-400 font-bold bg-rose-50 dark:bg-rose-950/40 px-3 py-1 rounded-full border border-rose-200 dark:border-rose-800 flex items-center gap-1.5 shadow-sm">
                  {t('products.outOfStock')}
                </span>
              )}
            </div>

            <div className="flex items-center gap-4">
              <div className="flex items-center border border-zinc-200 dark:border-zinc-800 rounded-2xl bg-zinc-50 dark:bg-zinc-900">
                <button
                  onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                  className="px-4 py-3 text-zinc-600 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-800 rounded-l-2xl font-bold"
                >
                  -
                </button>
                <span className="px-4 py-3 text-sm font-extrabold">{quantity}</span>
                <button
                  onClick={() => setQuantity((q) => q + 1)}
                  className="px-4 py-3 text-zinc-600 dark:text-zinc-300 hover:bg-zinc-200 dark:hover:bg-zinc-800 rounded-r-2xl font-bold"
                >
                  +
                </button>
              </div>

              <button
                disabled={!inStock}
                onClick={handleAddToCart}
                className="flex-1 py-3.5 px-6 rounded-2xl bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white hover:from-indigo-500 hover:to-pink-500 disabled:opacity-40 font-black text-xs uppercase tracking-wider shadow-xl shadow-indigo-500/25 hover:shadow-indigo-500/40 flex items-center justify-center gap-2.5 transition active:scale-95"
              >
                <ShoppingBag className="w-4 h-4" />
                {t('products.addToCart')}
              </button>
            </div>
          </div>

          {/* Badges / Guarantees */}
          <div className="grid grid-cols-2 gap-4 pt-6 border-t border-zinc-200 dark:border-zinc-800 text-xs text-zinc-600 dark:text-zinc-400">
            <div className="flex items-center gap-2.5">
              <Truck className="w-4 h-4 text-emerald-500" />
              <span className="font-medium">Worldwide express shipping</span>
            </div>
            <div className="flex items-center gap-2.5">
              <ShieldCheck className="w-4 h-4 text-indigo-500" />
              <span className="font-medium">100% Guaranteed authentic</span>
            </div>
          </div>
        </div>
      </div>

      {/* Description & Reviews */}
      <div className="border-t border-zinc-200 dark:border-zinc-800 pt-14 space-y-10">
        <div>
          <h2 className="text-xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 mb-4">
            Product Architecture & Overview
          </h2>
          <div className="prose dark:prose-invert max-w-none text-zinc-600 dark:text-zinc-300 text-xs sm:text-sm leading-relaxed font-light">
            {product.fullDescription || product.description || 'Crafted with premium materials and engineered to the highest specifications.'}
          </div>
        </div>

        {/* Reviews Section */}
        <div className="pt-10 border-t border-zinc-200 dark:border-zinc-800 space-y-8">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100">
              Customer Reviews ({reviews.length})
            </h3>
          </div>

          {/* Add Review Form */}
          <form
            onSubmit={handleReviewSubmit}
            className="p-6 sm:p-8 rounded-3xl bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 space-y-4 shadow-sm"
          >
            <h4 className="font-black text-sm uppercase tracking-wider text-indigo-600 dark:text-indigo-400">Leave a Review</h4>
            <div className="flex items-center gap-2">
              <span className="text-xs text-zinc-400 font-medium">Rating:</span>
              <div className="flex gap-1">
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    type="button"
                    key={star}
                    onClick={() => setNewRating(star)}
                    className="p-1"
                  >
                    <Star
                      className={`w-4 h-4 transition-colors ${
                        star <= newRating ? 'fill-amber-400 text-amber-400' : 'text-zinc-300 dark:text-zinc-700'
                      }`}
                    />
                  </button>
                ))}
              </div>
            </div>

            <input
              type="text"
              placeholder="Headline / Title (optional)"
              value={newTitle}
              onChange={(e) => setNewTitle(e.target.value)}
              className="w-full px-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 focus:ring-2 focus:ring-indigo-500"
            />
            <textarea
              rows={3}
              placeholder="Write your detailed feedback..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              required
              className="w-full px-4 py-2.5 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 focus:ring-2 focus:ring-indigo-500"
            />
            <button
              type="submit"
              disabled={submittingReview}
              className="px-6 py-2.5 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-full text-xs font-bold uppercase tracking-wider hover:opacity-90 shadow-md shadow-indigo-500/25 transition disabled:opacity-50"
            >
              {submittingReview ? 'Submitting...' : 'Submit Review'}
            </button>
          </form>

          {/* Reviews list */}
          <div className="space-y-4">
            {reviews.length > 0 ? (
              reviews.map((rev) => (
                <div
                  key={rev.id}
                  className="p-5 rounded-2xl bg-zinc-50 dark:bg-zinc-900/60 border border-zinc-200 dark:border-zinc-800 space-y-2 hover:border-indigo-300 dark:hover:border-indigo-800 transition-colors"
                >
                  <div className="flex items-center justify-between">
                    <span className="font-bold text-xs uppercase tracking-wide text-zinc-900 dark:text-zinc-100">{rev.userName}</span>
                    <div className="flex gap-1">
                      {[...Array(rev.rating)].map((_, i) => (
                        <Star key={i} className="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
                      ))}
                    </div>
                  </div>
                  {rev.title && <p className="font-bold text-xs text-zinc-900 dark:text-zinc-100">{rev.title}</p>}
                  <p className="text-xs text-zinc-600 dark:text-zinc-400 font-light">{rev.comment}</p>
                </div>
              ))
            ) : (
              <p className="text-xs text-zinc-400">{t('products.noReviewsYet')}</p>
            )}
          </div>
        </div>
      </div>

      {/* Related Products */}
      {relatedProducts.length > 0 && (
        <section className="space-y-6 pt-12 border-t border-zinc-200 dark:border-zinc-800">
          <h2 className="text-2xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <Sparkles className="w-5 h-5 text-indigo-500 dark:text-indigo-400" />
            {t('products.relatedProducts')}
          </h2>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-6">
            {relatedProducts.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}

      {/* Recently Viewed Products */}
      {recentlyViewed.length > 0 && (
        <section className="space-y-6 pt-12 border-t border-zinc-200 dark:border-zinc-800">
          <h2 className="text-2xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
            <Clock className="w-5 h-5 text-indigo-500 dark:text-indigo-400" />
            {t('products.recentlyViewed')}
          </h2>
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-6">
            {recentlyViewed.filter((p) => p.id !== product.id).slice(0, 4).map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
};
