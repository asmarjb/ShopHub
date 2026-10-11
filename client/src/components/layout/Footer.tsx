import React from 'react';
import { Link } from 'react-router-dom';
import { ShoppingBag, ArrowUpRight } from 'lucide-react';

export const Footer: React.FC = () => {
  return (
    <footer className="bg-white dark:bg-zinc-950 border-t border-zinc-200 dark:border-zinc-800 transition-colors mt-auto text-zinc-900 dark:text-zinc-100">
      <div className="max-w-7xl mx-auto px-4 py-16 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-10">
          
          {/* Brand Column */}
          <div className="space-y-4">
            <Link to="/" className="flex items-center gap-2.5">
              <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-indigo-600 via-purple-600 to-pink-500 flex items-center justify-center text-white shadow-md shadow-indigo-500/25">
                <ShoppingBag className="w-4 h-4" />
              </div>
              <span className="font-black text-xl tracking-wider uppercase font-sans bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 bg-clip-text text-transparent dark:from-indigo-400 dark:via-purple-400 dark:to-pink-400">
                ATELIER
              </span>
            </Link>
            <p className="text-xs text-zinc-500 dark:text-zinc-400 leading-relaxed font-light">
              Architected for the modern era. Curated collections of premium technology, timeless apparel, and contemporary living essentials.
            </p>
          </div>

          {/* Catalog Links */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-widest text-indigo-600 dark:text-indigo-400">
              Collections
            </h4>
            <ul className="space-y-2.5 text-xs font-medium text-zinc-600 dark:text-zinc-300">
              <li>
                <Link to="/products" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  All Products
                </Link>
              </li>
              <li>
                <Link to="/categories" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Categories
                </Link>
              </li>
              <li>
                <Link to="/products?featured=true" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Featured Items
                </Link>
              </li>
            </ul>
          </div>

          {/* Client Care */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-widest text-indigo-600 dark:text-indigo-400">
              Client Care
            </h4>
            <ul className="space-y-2.5 text-xs font-medium text-zinc-600 dark:text-zinc-300">
              <li>
                <Link to="/my-orders" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Order Tracking
                </Link>
              </li>
              <li>
                <Link to="/wishlist" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Saved Wishlist
                </Link>
              </li>
              <li>
                <Link to="/cart" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
                  Shopping Bag
                </Link>
              </li>
            </ul>
          </div>

          {/* Newsletter / Contact */}
          <div className="space-y-3">
            <h4 className="text-xs font-bold uppercase tracking-widest text-indigo-600 dark:text-indigo-400">
              Newsletter
            </h4>
            <p className="text-xs text-zinc-500 dark:text-zinc-400 font-light">
              Receive private invitations to exclusive sales and catalog drops.
            </p>
            <div className="flex gap-2 pt-1">
              <input
                type="email"
                placeholder="Enter your email"
                className="w-full px-3.5 py-2 text-xs rounded-full border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900 text-zinc-900 dark:text-zinc-100 placeholder-zinc-400 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
              <button
                type="button"
                className="px-4 py-2 rounded-full bg-gradient-to-r from-indigo-600 to-purple-600 text-white text-xs font-bold uppercase tracking-wider hover:opacity-90 shadow-md shadow-indigo-500/25 transition-opacity"
              >
                Join
              </button>
            </div>
          </div>
        </div>

        {/* Bottom Credits */}
        <div className="border-t border-zinc-200 dark:border-zinc-800 mt-12 pt-8 flex flex-col sm:flex-row items-center justify-between text-xs text-zinc-400 gap-4">
          <p>&copy; {new Date().getFullYear()} ATELIER E-Commerce. All rights reserved.</p>
          <div className="flex items-center gap-6">
            <a href="http://localhost:5169/scalar/v1" target="_blank" rel="noreferrer" className="text-indigo-500 dark:text-indigo-400 hover:text-indigo-600 flex items-center gap-1 font-semibold transition-colors">
              <span>Scalar API Docs</span>
              <ArrowUpRight className="w-3 h-3" />
            </a>
            <span className="hover:text-zinc-600 dark:hover:text-zinc-300 cursor-pointer">Privacy Policy</span>
            <span className="hover:text-zinc-600 dark:hover:text-zinc-300 cursor-pointer">Terms of Service</span>
          </div>
        </div>
      </div>
    </footer>
  );
};
