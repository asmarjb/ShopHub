import React, { useEffect, useState, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Filter, SlidersHorizontal, ChevronDown, ChevronRight, RefreshCw } from 'lucide-react';
import { productService } from '../../services/productService';
import { Product, Category, Brand, PagedResult } from '../../types';
import { ProductCard } from '../../components/product/ProductCard';

export const ProductList: React.FC = () => {
  const { t } = useTranslation();
  const [searchParams, setSearchParams] = useSearchParams();
  const sidebarRef = useRef<HTMLElement>(null);

  const [productsData, setProductsData] = useState<PagedResult<Product> | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  // Search & Filter state
  const searchTerm = searchParams.get('search') || '';
  const categoryId = searchParams.get('categoryId') || searchParams.get('category') || '';
  const brandId = searchParams.get('brandId') || searchParams.get('brand') || '';
  const minPrice = searchParams.get('minPrice') || '';
  const maxPrice = searchParams.get('maxPrice') || '';
  const sortBy = searchParams.get('sortBy') || 'createdAt';
  const isDescending = searchParams.get('isDescending') !== 'false';
  const pageNumber = parseInt(searchParams.get('page') || '1', 10);

  // Temporary price inputs
  const [priceMin, setPriceMin] = useState(minPrice);
  const [priceMax, setPriceMax] = useState(maxPrice);

  // Hierarchical categories expansion
  const [expandedCategories, setExpandedCategories] = useState<Record<string, boolean>>({});

  const toggleCategoryExpand = (catId: string, e?: React.MouseEvent) => {
    if (e) e.stopPropagation();
    setExpandedCategories((prev) => ({
      ...prev,
      [catId]: !prev[catId],
    }));
  };

  useEffect(() => {
    const fetchMetadata = async () => {
      try {
        const [cats, brs] = await Promise.all([
          productService.getCategories(),
          productService.getBrands(),
        ]);
        const filteredCats = cats.filter((c) => c.slug !== 'archive-category');
        setCategories(filteredCats);
        setBrands(brs);

        // Auto-expand parents that contain active category
        if (categoryId) {
          const selected = filteredCats.find((c) => c.id === categoryId);
          if (selected?.parentCategoryId) {
            setExpandedCategories((prev) => ({ ...prev, [selected.parentCategoryId!]: true }));
          } else if (selected && !selected.parentCategoryId) {
            setExpandedCategories((prev) => ({ ...prev, [selected.id]: true }));
          }
        }
      } catch {
        // quiet error
      }
    };
    fetchMetadata();
  }, [categoryId]);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      try {
        const data = await productService.getProducts({
          pageNumber,
          pageSize: 12,
          searchTerm: searchTerm || undefined,
          categoryId: categoryId || undefined,
          brandId: brandId || undefined,
          minPrice: minPrice ? parseFloat(minPrice) : undefined,
          maxPrice: maxPrice ? parseFloat(maxPrice) : undefined,
          sortBy,
          isDescending,
        });
        setProductsData(data);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [searchTerm, categoryId, brandId, minPrice, maxPrice, sortBy, isDescending, pageNumber]);

  // Restore sidebar scroll position on load and updates
  useEffect(() => {
    const saved = sessionStorage.getItem('catalog_sidebar_scroll');
    if (saved && sidebarRef.current) {
      sidebarRef.current.scrollTop = parseInt(saved, 10);
    }
  }, [categoryId, brandId, productsData]);

  const updateParam = (key: string, value: string | null) => {
    const newParams = new URLSearchParams(searchParams);
    if (value) {
      newParams.set(key, value);
    } else {
      newParams.delete(key);
    }
    newParams.set('page', '1');
    setSearchParams(newParams);
  };

  const applyPriceFilter = () => {
    const newParams = new URLSearchParams(searchParams);
    if (priceMin) newParams.set('minPrice', priceMin);
    else newParams.delete('minPrice');

    if (priceMax) newParams.set('maxPrice', priceMax);
    else newParams.delete('maxPrice');

    newParams.set('page', '1');
    setSearchParams(newParams);
  };

  const clearAllFilters = () => {
    setPriceMin('');
    setPriceMax('');
    setSearchParams(new URLSearchParams());
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-8">
      {/* Top Header & Sort */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border-b border-zinc-200 dark:border-zinc-800 pb-6">
        <div>
          <h1 className="text-3xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans">
            {t('products.title')}
          </h1>
          {searchTerm ? (
            <p className="text-xs font-medium text-zinc-500 dark:text-zinc-400 mt-1.5">
              Search results for "{searchTerm}" ({productsData?.totalCount || 0} items)
            </p>
          ) : (
            <p className="text-xs font-light text-zinc-500 dark:text-zinc-400 mt-1.5">
              Showing {productsData?.totalCount || 0} curated items
            </p>
          )}
        </div>

        <div className="flex items-center gap-3 w-full sm:w-auto">
          {/* Mobile Filter Toggle */}
          <button
            onClick={() => setIsFilterOpen((prev) => !prev)}
            className="lg:hidden flex-1 sm:flex-none flex items-center justify-center gap-2 px-4 py-2.5 text-xs font-bold uppercase tracking-wider rounded-full border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 text-zinc-800 dark:text-zinc-200"
          >
            <SlidersHorizontal className="w-4 h-4" />
            {t('products.filter')}
          </button>

          {/* Sort Dropdown */}
          <div className="relative flex-1 sm:flex-none">
            <select
              value={`${sortBy}_${isDescending}`}
              onChange={(e) => {
                const [sb, desc] = e.target.value.split('_');
                const newParams = new URLSearchParams(searchParams);
                newParams.set('sortBy', sb);
                newParams.set('isDescending', desc);
                setSearchParams(newParams);
              }}
              className="w-full appearance-none px-5 py-2.5 pr-10 text-xs font-bold uppercase tracking-wider rounded-full border border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-900 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-1 focus:ring-black dark:focus:ring-white"
            >
              <option value="createdAt_true">{t('products.newest')}</option>
              <option value="basePrice_false">{t('products.priceLowHigh')}</option>
              <option value="basePrice_true">{t('products.priceHighLow')}</option>
              <option value="name_false">{t('products.nameAsc')}</option>
            </select>
            <ChevronDown className="w-4 h-4 text-zinc-400 absolute right-3.5 top-3.5 pointer-events-none" />
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
        {/* Sidebar Filters */}
        <aside
          ref={sidebarRef}
          onScroll={(e) => sessionStorage.setItem('catalog_sidebar_scroll', String(e.currentTarget.scrollTop))}
          className={`lg:block ${
            isFilterOpen ? 'block' : 'hidden'
          } space-y-7 bg-white dark:bg-zinc-900 p-6 rounded-3xl border border-zinc-200 dark:border-zinc-800 h-fit lg:sticky lg:top-24 lg:max-h-[calc(100vh-7rem)] lg:overflow-y-auto scrollbar-thin scrollbar-thumb-zinc-300 dark:scrollbar-thumb-zinc-700`}
        >
          <div className="flex items-center justify-between pb-3.5 border-b border-zinc-100 dark:border-zinc-800">
            <span className="font-bold text-xs uppercase tracking-wider text-zinc-900 dark:text-zinc-100 flex items-center gap-2">
              <Filter className="w-4 h-4 text-indigo-500" />
              {t('products.filter')}
            </span>
            <button
              onClick={clearAllFilters}
              className="text-xs text-indigo-600 dark:text-indigo-400 hover:underline font-semibold"
            >
              {t('products.clearFilter')}
            </button>
          </div>

          {/* Categories Hierarchical Accordion */}
          <div>
            <h3 className="text-[11px] font-black uppercase tracking-widest text-indigo-600 dark:text-indigo-400 mb-3">
              {t('products.categories')}
            </h3>
            <div className="space-y-1.5 text-xs">
              {/* All Categories Option */}
              <button
                type="button"
                onClick={() => updateParam('categoryId', null)}
                className={`w-full text-left px-3 py-2 rounded-xl transition font-semibold ${
                  !categoryId
                    ? 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/20'
                    : 'text-zinc-600 dark:text-zinc-300 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 hover:text-indigo-600'
                }`}
              >
                {t('products.allCategories')}
              </button>

              {/* Grouped Parent & Subcategories */}
              {categories
                .filter((c) => !c.parentCategoryId)
                .map((pCat) => {
                  const subCats = categories.filter((s) => s.parentCategoryId === pCat.id);
                  const hasSub = subCats.length > 0;
                  const isParentActive = categoryId === pCat.id;
                  const isSubActive = subCats.some((s) => s.id === categoryId);
                  const isExpanded = expandedCategories[pCat.id] ?? (isParentActive || isSubActive);

                  if (hasSub) {
                    return (
                      <div key={pCat.id} className="space-y-1">
                        {/* Parent Category Row */}
                        <div
                          className={`flex items-center justify-between rounded-xl transition text-xs font-semibold ${
                            isParentActive
                              ? 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/20'
                              : isSubActive
                              ? 'bg-indigo-50/80 dark:bg-indigo-950/40 text-indigo-600 dark:text-indigo-400 font-bold'
                              : 'text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800'
                          }`}
                        >
                          <button
                            type="button"
                            onClick={() => updateParam('categoryId', pCat.id)}
                            className="flex-1 text-left px-3 py-2 truncate"
                            title={`Show all ${pCat.name}`}
                          >
                            {pCat.name}
                          </button>
                          <button
                            type="button"
                            onClick={(e) => toggleCategoryExpand(pCat.id, e)}
                            className={`px-2.5 py-2 transition-transform ${
                              isParentActive ? 'text-white' : 'text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200'
                            }`}
                            title={isExpanded ? 'Collapse' : 'Expand'}
                          >
                            {isExpanded ? (
                              <ChevronDown className="w-3.5 h-3.5" />
                            ) : (
                              <ChevronRight className="w-3.5 h-3.5" />
                            )}
                          </button>
                        </div>

                        {/* Subcategories */}
                        {isExpanded && (
                          <div className="ml-3 pl-2.5 border-l-2 border-indigo-200 dark:border-indigo-900/60 space-y-1 py-0.5">
                            <button
                              type="button"
                              onClick={() => updateParam('categoryId', pCat.id)}
                              className={`w-full text-left px-2 py-1 rounded-lg text-[11px] transition ${
                                isParentActive
                                  ? 'font-bold text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/60'
                                  : 'text-zinc-500 dark:text-zinc-400 hover:text-indigo-600 dark:hover:text-indigo-300'
                              }`}
                            >
                              All {pCat.name}
                            </button>
                            {subCats.map((sub) => {
                              const isCurrent = categoryId === sub.id;
                              return (
                                <button
                                  key={sub.id}
                                  type="button"
                                  onClick={() => updateParam('categoryId', sub.id)}
                                  className={`w-full text-left px-2 py-1 rounded-lg text-[11px] transition font-medium ${
                                    isCurrent
                                      ? 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white font-bold shadow-sm'
                                      : 'text-zinc-600 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 hover:text-indigo-600'
                                  }`}
                                >
                                  {sub.name}
                                </button>
                              );
                            })}
                          </div>
                        )}
                      </div>
                    );
                  }

                  {/* Standalone Parent Category */}
                  return (
                    <button
                      key={pCat.id}
                      type="button"
                      onClick={() => updateParam('categoryId', pCat.id)}
                      className={`w-full text-left px-3 py-2 rounded-xl transition font-semibold ${
                        isParentActive
                          ? 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/20'
                          : 'text-zinc-600 dark:text-zinc-300 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 hover:text-indigo-600'
                      }`}
                    >
                      {pCat.name}
                    </button>
                  );
                })}
            </div>
          </div>

          {/* Brands */}
          {brands.length > 0 && (
            <div>
              <h3 className="text-[11px] font-black uppercase tracking-widest text-indigo-600 dark:text-indigo-400 mb-3">
                {t('products.brands')}
              </h3>
              <ul className="space-y-1 text-xs">
                <li>
                  <button
                    onClick={() => updateParam('brandId', null)}
                    className={`w-full text-left px-3 py-2 rounded-xl transition font-semibold ${
                      !brandId
                        ? 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/20'
                        : 'text-zinc-600 dark:text-zinc-300 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 hover:text-indigo-600'
                    }`}
                  >
                    {t('products.allBrands')}
                  </button>
                </li>
                {brands.map((b) => (
                  <li key={b.id}>
                    <button
                      onClick={() => updateParam('brandId', b.id)}
                      className={`w-full text-left px-3 py-2 rounded-xl transition font-semibold ${
                        brandId === b.id
                          ? 'bg-gradient-to-r from-indigo-600 to-purple-600 text-white shadow-md shadow-indigo-500/20'
                          : 'text-zinc-600 dark:text-zinc-300 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 hover:text-indigo-600'
                      }`}
                    >
                      {b.name}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {/* Price Range */}
          <div>
            <h3 className="text-[11px] font-black uppercase tracking-widest text-indigo-600 dark:text-indigo-400 mb-3">
              {t('products.priceRange')}
            </h3>
            <div className="flex items-center gap-2">
              <input
                type="number"
                placeholder={t('products.minPrice')}
                value={priceMin}
                onChange={(e) => setPriceMin(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
              <span className="text-zinc-400">-</span>
              <input
                type="number"
                placeholder={t('products.maxPrice')}
                value={priceMax}
                onChange={(e) => setPriceMax(e.target.value)}
                className="w-full px-3 py-2 text-xs rounded-xl border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-950 text-zinc-900 dark:text-zinc-100 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
            </div>
            <button
              onClick={applyPriceFilter}
              className="mt-3.5 w-full py-2.5 bg-gradient-to-r from-indigo-600 via-purple-600 to-indigo-700 text-white rounded-xl text-xs font-bold uppercase tracking-wider hover:opacity-95 shadow-md shadow-indigo-500/25 transition"
            >
              {t('products.applyFilter')}
            </button>
          </div>
        </aside>

        {/* Product Grid Area */}
        <main className="lg:col-span-3 space-y-8">
          {loading ? (
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-6">
              {[...Array(6)].map((_, i) => (
                <div
                  key={i}
                  className="aspect-square rounded-2xl bg-zinc-100 dark:bg-zinc-800 animate-pulse"
                />
              ))}
            </div>
          ) : productsData && productsData.items.length > 0 ? (
            <>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-6">
                {productsData.items.map((prod) => (
                  <ProductCard key={prod.id} product={prod} />
                ))}
              </div>

              {/* Pagination */}
              {productsData.totalPages > 1 && (
                <div className="flex items-center justify-center gap-3 pt-8 border-t border-zinc-200 dark:border-zinc-800">
                  <button
                    disabled={!productsData.hasPreviousPage}
                    onClick={() => updateParam('page', (pageNumber - 1).toString())}
                    className="px-5 py-2.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-xs font-bold uppercase tracking-wider disabled:opacity-30 disabled:cursor-not-allowed hover:bg-zinc-100 dark:hover:bg-zinc-800 transition"
                  >
                    Previous
                  </button>
                  <span className="px-4 text-xs font-semibold text-zinc-500 dark:text-zinc-400">
                    Page {productsData.pageNumber} of {productsData.totalPages}
                  </span>
                  <button
                    disabled={!productsData.hasNextPage}
                    onClick={() => updateParam('page', (pageNumber + 1).toString())}
                    className="px-5 py-2.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-xs font-bold uppercase tracking-wider disabled:opacity-30 disabled:cursor-not-allowed hover:bg-zinc-100 dark:hover:bg-zinc-800 transition"
                  >
                    Next
                  </button>
                </div>
              )}
            </>
          ) : (
            <div className="text-center py-20 bg-white dark:bg-zinc-900 rounded-3xl border border-zinc-200 dark:border-zinc-800 p-8 space-y-4">
              <RefreshCw className="w-10 h-10 text-zinc-400 mx-auto" />
              <h3 className="text-base font-bold text-zinc-900 dark:text-zinc-100">
                {t('products.noProductsFound')}
              </h3>
              <p className="text-xs text-zinc-500 dark:text-zinc-400 max-w-sm mx-auto">
                {t('products.noProductsFoundSub')}
              </p>
              <button
                onClick={clearAllFilters}
                className="px-6 py-2.5 bg-black text-white dark:bg-white dark:text-black rounded-full text-xs font-bold uppercase tracking-wider"
              >
                {t('products.clearFilter')}
              </button>
            </div>
          )}
        </main>
      </div>
    </div>
  );
};
