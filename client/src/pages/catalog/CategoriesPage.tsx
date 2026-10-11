import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ArrowRight, Layers } from 'lucide-react';
import { productService } from '../../services/productService';
import { Category } from '../../types';

export const CategoriesPage: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCats = async () => {
      try {
        const data = await productService.getCategories();
        setCategories(data.filter((c) => c.slug !== 'archive-category'));
      } finally {
        setLoading(false);
      }
    };
    fetchCats();
  }, []);

  const parentCategories = categories.filter((c) => !c.parentCategoryId);

  const gradients = [
    'from-indigo-600 to-blue-500 shadow-indigo-500/25',
    'from-purple-600 to-pink-500 shadow-purple-500/25',
    'from-emerald-500 to-teal-600 shadow-emerald-500/25',
    'from-amber-500 to-orange-600 shadow-amber-500/25',
    'from-rose-500 to-red-600 shadow-rose-500/25',
    'from-cyan-500 to-blue-600 shadow-cyan-500/25',
    'from-violet-600 to-fuchsia-600 shadow-violet-500/25',
  ];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-10">
      <div className="border-b border-zinc-200 dark:border-zinc-800 pb-6">
        <span className="text-[10px] font-black uppercase tracking-widest text-indigo-600 dark:text-indigo-400">
          CATALOG ARCHITECTURE
        </span>
        <h1 className="text-3xl sm:text-4xl font-black uppercase tracking-tight text-zinc-900 dark:text-zinc-100 font-sans mt-1">
          Explore All Categories
        </h1>
        <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-1 font-light">
          Discover curated collections, distinct departments, and sub-categories.
        </p>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
          {[...Array(6)].map((_, i) => (
            <div key={i} className="h-44 bg-zinc-100 dark:bg-zinc-900 rounded-3xl border border-zinc-200 dark:border-zinc-800" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {parentCategories.map((cat, idx) => {
            const subCategories = categories.filter((c) => c.parentCategoryId === cat.id);
            const grad = gradients[idx % gradients.length];

            return (
              <div
                key={cat.id}
                className="p-7 bg-white dark:bg-zinc-900 rounded-3xl border border-zinc-200 dark:border-zinc-800 hover:border-indigo-400 dark:hover:border-indigo-600 hover:shadow-xl hover:shadow-indigo-500/10 transition-all duration-300 shadow-sm flex flex-col justify-between space-y-6"
              >
                <div>
                  <div className="flex items-center justify-between mb-3">
                    <span className={`p-2.5 rounded-xl bg-gradient-to-tr ${grad} text-white shadow-md`}>
                      <Layers className="w-4 h-4" />
                    </span>
                    <Link
                      to={`/products?categoryId=${cat.id}`}
                      className="text-xs font-bold uppercase tracking-wider text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 flex items-center gap-1 transition-colors"
                    >
                      <span>View All</span>
                      <ArrowRight className="w-3.5 h-3.5" />
                    </Link>
                  </div>

                  <Link
                    to={`/products?categoryId=${cat.id}`}
                    className="font-black text-xl text-zinc-900 dark:text-zinc-100 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors tracking-tight"
                  >
                    {cat.name}
                  </Link>

                  {cat.description && (
                    <p className="text-xs text-zinc-500 dark:text-zinc-400 mt-2 font-light leading-relaxed">
                      {cat.description}
                    </p>
                  )}
                </div>

                {subCategories.length > 0 && (
                  <div className="pt-4 border-t border-zinc-100 dark:border-zinc-800">
                    <span className="text-[10px] font-black uppercase tracking-widest text-zinc-400 block mb-2">
                      Sub-departments
                    </span>
                    <div className="flex flex-wrap gap-2">
                      {subCategories.map((sub) => (
                        <Link
                          key={sub.id}
                          to={`/products?categoryId=${sub.id}`}
                          className="px-3 py-1.5 rounded-full text-xs font-semibold bg-indigo-50/70 dark:bg-indigo-950/40 text-indigo-700 dark:text-indigo-300 hover:bg-gradient-to-r hover:from-indigo-600 hover:to-purple-600 hover:text-white transition-all shadow-sm"
                        >
                          {sub.name}
                        </Link>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
