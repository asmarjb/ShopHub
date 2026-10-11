import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  ArrowRight,
  Sparkles,
  ChevronLeft,
  ChevronRight,
  Truck,
  ShieldCheck,
  RefreshCw,
  Headphones,
} from 'lucide-react';
import { productService } from '../services/productService';
import { Product, Banner, Category } from '../types';
import { ProductCard } from '../components/product/ProductCard';

export const Home: React.FC = () => {
  const { t } = useTranslation();
  const [banners, setBanners] = useState<Banner[]>([]);
  const [featuredProducts, setFeaturedProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [currentBannerIndex, setCurrentBannerIndex] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadHomeData = async () => {
      try {
        const [bannersData, productsRes, categoriesData] = await Promise.all([
          productService.getBanners().catch(() => []),
          productService.getProducts({ pageSize: 8 }).catch(() => ({ items: [] })),
          productService.getCategories().catch(() => []),
        ]);

        const safeBanners = Array.isArray(bannersData) ? bannersData : (bannersData as any)?.data || [];
        const safeCategories = Array.isArray(categoriesData) ? categoriesData : (categoriesData as any)?.data || [];
        const safeProducts = productsRes?.items || (Array.isArray(productsRes) ? productsRes : []);

        setBanners(safeBanners.filter((b: any) => b && b.isActive));
        setFeaturedProducts(safeProducts);
        // filter out internal archive category
        setCategories(safeCategories.filter((c: any) => c && c.slug !== 'archive-category').slice(0, 8));
      } finally {
        setLoading(false);
      }
    };

    loadHomeData();
  }, []);

  // Banner slider automatic cycle
  useEffect(() => {
    if (banners.length <= 1) return;
    const interval = setInterval(() => {
      setCurrentBannerIndex((prev) => (prev + 1) % banners.length);
    }, 6000);
    return () => clearInterval(interval);
  }, [banners.length]);

  return (
    <div className="space-y-20 pb-20">
      {/* Hero / Banner Slider - Vibrant Modern Design */}
      <section className="relative overflow-hidden bg-gradient-to-br from-slate-950 via-indigo-950 to-slate-900 text-white rounded-3xl mx-4 sm:mx-6 lg:mx-8 mt-6 shadow-2xl border border-indigo-950/60">
        {/* Colorful ambient glow effects */}
        <div className="absolute -top-24 -left-24 w-96 h-96 bg-indigo-500/20 rounded-full blur-3xl pointer-events-none" />
        <div className="absolute -bottom-24 -right-24 w-96 h-96 bg-purple-500/20 rounded-full blur-3xl pointer-events-none" />
        <div className="absolute top-1/2 left-1/3 w-64 h-64 bg-pink-500/15 rounded-full blur-3xl pointer-events-none" />

        {banners.length > 0 ? (
          <div className="relative min-h-[460px] sm:min-h-[520px] lg:min-h-[560px] flex items-center">
            {banners.map((banner, index) => (
              <div
                key={banner.id}
                className={`absolute inset-0 transition-opacity duration-1000 flex items-center ${
                  index === currentBannerIndex ? 'opacity-100 z-10' : 'opacity-0 z-0'
                }`}
              >
                <img
                  src={banner.imageUrl}
                  alt={banner.title}
                  className="absolute inset-0 w-full h-full object-cover opacity-45 mix-blend-screen"
                />
                <div className="absolute inset-0 bg-gradient-to-r from-slate-950 via-slate-950/80 to-transparent" />
                <div className="relative max-w-2xl px-8 sm:px-16 space-y-6">
                  <span className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full text-xs font-bold tracking-widest uppercase bg-gradient-to-r from-indigo-500/20 to-purple-500/20 border border-indigo-400/30 text-indigo-200 backdrop-blur-md">
                    <Sparkles className="w-3.5 h-3.5 text-amber-300" />
                    {t('home.heroTag')}
                  </span>
                  <h1 className="text-4xl sm:text-6xl font-black tracking-tight leading-tight uppercase font-sans">
                    <span className="bg-gradient-to-r from-white via-indigo-100 to-pink-200 bg-clip-text text-transparent">
                      {banner.title}
                    </span>
                  </h1>
                  <p className="text-sm sm:text-base text-indigo-100/90 leading-relaxed max-w-lg font-light">
                    {banner.subTitle || t('home.heroSubtitle')}
                  </p>
                  <div className="pt-2 flex flex-wrap gap-4">
                    <Link
                      to={banner.linkUrl || '/products'}
                      className="inline-flex items-center gap-2.5 px-7 py-3.5 rounded-full bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white font-extrabold text-xs uppercase tracking-wider hover:from-indigo-500 hover:to-pink-500 shadow-xl shadow-indigo-500/30 transition-all transform hover:-translate-y-0.5 hover:scale-105"
                    >
                      {t('home.shopNow')}
                      <ArrowRight className="w-4 h-4" />
                    </Link>
                    <Link
                      to="/categories"
                      className="inline-flex items-center gap-2 px-6 py-3.5 rounded-full bg-white/10 hover:bg-white/20 border border-white/20 text-white font-bold text-xs uppercase tracking-wider transition-colors backdrop-blur-md"
                    >
                      {t('home.exploreMore')}
                    </Link>
                  </div>
                </div>
              </div>
            ))}

            {/* Slider controls */}
            {banners.length > 1 && (
              <div className="absolute bottom-8 right-12 z-20 flex gap-3">
                <button
                  onClick={() =>
                    setCurrentBannerIndex((prev) => (prev === 0 ? banners.length - 1 : prev - 1))
                  }
                  className="p-3 rounded-full bg-slate-900/80 border border-indigo-900/50 hover:bg-gradient-to-r hover:from-indigo-600 hover:to-purple-600 hover:border-transparent text-white transition-all shadow-md"
                  aria-label="Previous slide"
                >
                  <ChevronLeft className="w-4 h-4" />
                </button>
                <button
                  onClick={() => setCurrentBannerIndex((prev) => (prev + 1) % banners.length)}
                  className="p-3 rounded-full bg-slate-900/80 border border-indigo-900/50 hover:bg-gradient-to-r hover:from-indigo-600 hover:to-purple-600 hover:border-transparent text-white transition-all shadow-md"
                  aria-label="Next slide"
                >
                  <ChevronRight className="w-4 h-4" />
                </button>
              </div>
            )}
          </div>
        ) : (
          <div className="min-h-[460px] flex items-center px-8 sm:px-16">
            <div className="max-w-2xl space-y-6">
              <span className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full text-xs font-bold tracking-widest uppercase bg-gradient-to-r from-indigo-500/20 to-purple-500/20 border border-indigo-400/30 text-indigo-200">
                {t('home.heroTag')}
              </span>
              <h1 className="text-4xl sm:text-6xl font-black tracking-tight leading-tight uppercase">
                <span className="bg-gradient-to-r from-white via-indigo-100 to-pink-200 bg-clip-text text-transparent">
                  CURATED EXCELLENCE
                </span>
              </h1>
              <p className="text-sm sm:text-base text-indigo-100/90 font-light max-w-lg">
                {t('home.heroSubtitle')}
              </p>
              <Link
                to="/products"
                className="inline-flex items-center gap-2.5 px-7 py-3.5 rounded-full bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white font-extrabold text-xs uppercase tracking-wider hover:opacity-95 shadow-xl shadow-indigo-500/30"
              >
                {t('home.shopNow')}
                <ArrowRight className="w-4 h-4" />
              </Link>
            </div>
          </div>
        )}
      </section>

      {/* Trust Badges / Perks - Vibrant Grid */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 flex items-start gap-4 hover:border-emerald-300 dark:hover:border-emerald-700/60 hover:shadow-lg hover:shadow-emerald-500/5 transition-all">
            <div className="p-3 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 text-white flex-shrink-0 shadow-md shadow-emerald-500/25">
              <Truck className="w-5 h-5" />
            </div>
            <div>
              <h4 className="font-bold text-sm text-zinc-900 dark:text-zinc-100">{t('home.perks.shipping')}</h4>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1 leading-relaxed">{t('home.perks.shippingDesc')}</p>
            </div>
          </div>

          <div className="p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 flex items-start gap-4 hover:border-indigo-300 dark:hover:border-indigo-700/60 hover:shadow-lg hover:shadow-indigo-500/5 transition-all">
            <div className="p-3 rounded-xl bg-gradient-to-br from-indigo-500 to-blue-600 text-white flex-shrink-0 shadow-md shadow-indigo-500/25">
              <ShieldCheck className="w-5 h-5" />
            </div>
            <div>
              <h4 className="font-bold text-sm text-zinc-900 dark:text-zinc-100">{t('home.perks.security')}</h4>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1 leading-relaxed">{t('home.perks.securityDesc')}</p>
            </div>
          </div>

          <div className="p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 flex items-start gap-4 hover:border-amber-300 dark:hover:border-amber-700/60 hover:shadow-lg hover:shadow-amber-500/5 transition-all">
            <div className="p-3 rounded-xl bg-gradient-to-br from-amber-500 to-orange-600 text-white flex-shrink-0 shadow-md shadow-amber-500/25">
              <RefreshCw className="w-5 h-5" />
            </div>
            <div>
              <h4 className="font-bold text-sm text-zinc-900 dark:text-zinc-100">{t('home.perks.returns')}</h4>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1 leading-relaxed">{t('home.perks.returnsDesc')}</p>
            </div>
          </div>

          <div className="p-6 rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 flex items-start gap-4 hover:border-purple-300 dark:hover:border-purple-700/60 hover:shadow-lg hover:shadow-purple-500/5 transition-all">
            <div className="p-3 rounded-xl bg-gradient-to-br from-purple-500 to-pink-600 text-white flex-shrink-0 shadow-md shadow-purple-500/25">
              <Headphones className="w-5 h-5" />
            </div>
            <div>
              <h4 className="font-bold text-sm text-zinc-900 dark:text-zinc-100">{t('home.perks.support')}</h4>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1 leading-relaxed">{t('home.perks.supportDesc')}</p>
            </div>
          </div>
        </div>
      </section>

      {/* Curated Categories */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 space-y-8">
        <div className="flex items-end justify-between border-b border-zinc-200 dark:border-zinc-800 pb-4">
          <div>
            <h2 className="text-2xl sm:text-3xl font-black text-zinc-900 dark:text-zinc-100 tracking-tight uppercase">
              {t('home.popularCategories')}
            </h2>
            <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
              {t('home.categoriesSubtitle')}
            </p>
          </div>
          <Link
            to="/categories"
            className="hidden sm:inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 transition-colors"
          >
            {t('home.viewAll')}
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4 sm:gap-6">
          {categories.map((cat) => (
            <Link
              key={cat.id}
              to={`/products?categoryId=${cat.id}`}
              className="group relative overflow-hidden rounded-2xl border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 p-6 flex flex-col justify-between hover:border-indigo-400 dark:hover:border-indigo-600 transition-all duration-300 shadow-sm hover:shadow-lg hover:shadow-indigo-500/10"
            >
              <div>
                <span className="text-[10px] font-bold text-indigo-600 dark:text-indigo-400 uppercase tracking-widest">
                  COLLECTION
                </span>
                <h3 className="font-extrabold text-base sm:text-lg text-zinc-900 dark:text-zinc-100 mt-1 group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
                  {cat.name}
                </h3>
                {cat.description && (
                  <p className="text-xs text-zinc-500 dark:text-zinc-400 line-clamp-2 mt-1.5 font-light">
                    {cat.description}
                  </p>
                )}
              </div>
              <div className="pt-6 flex items-center text-xs font-bold text-indigo-600 dark:text-indigo-400 gap-1 group-hover:translate-x-1 transition-transform">
                <span>Explore</span>
                <ArrowRight className="w-3.5 h-3.5" />
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* Featured Products */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 space-y-8">
        <div className="flex items-end justify-between border-b border-zinc-200 dark:border-zinc-800 pb-4">
          <div>
            <h2 className="text-2xl sm:text-3xl font-black text-zinc-900 dark:text-zinc-100 tracking-tight uppercase">
              {t('home.featuredProducts')}
            </h2>
            <p className="text-sm text-zinc-500 dark:text-zinc-400 mt-1">
              {t('home.featuredSubtitle')}
            </p>
          </div>
          <Link
            to="/products"
            className="inline-flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 transition-colors"
          >
            {t('home.viewAll')}
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>

        {loading ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
            {[...Array(8)].map((_, i) => (
              <div
                key={i}
                className="aspect-square bg-zinc-100 dark:bg-zinc-800 animate-pulse rounded-2xl"
              />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
            {featuredProducts.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        )}
      </section>
    </div>
  );
};
