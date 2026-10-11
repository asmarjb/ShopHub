import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Search,
  ShoppingCart,
  Heart,
  User,
  Sun,
  Moon,
  Globe,
  Menu,
  X,
  LogOut,
  ShoppingBag,
  ShieldCheck,
  Package,
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';
import { useWishlist } from '../../context/WishlistContext';
import { useCart } from '../../context/CartContext';
import { productService, AutocompleteProduct } from '../../services/productService';
import { useDebounce } from '../../hooks/useDebounce';

export const Navbar: React.FC = () => {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const { count: wishlistCount } = useWishlist();
  const { itemsCount } = useCart();

  const [searchQuery, setSearchQuery] = useState('');
  const [suggestions, setSuggestions] = useState<AutocompleteProduct[]>([]);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [profileDropdownOpen, setProfileDropdownOpen] = useState(false);

  const searchRef = useRef<HTMLDivElement>(null);
  const debouncedSearch = useDebounce(searchQuery, 300);

  useEffect(() => {
    const fetchSuggestions = async () => {
      if (debouncedSearch.trim().length >= 2) {
        try {
          const res = await productService.getAutocomplete(debouncedSearch.trim());
          setSuggestions(Array.isArray(res) ? res : []);
          setShowSuggestions(true);
        } catch {
          setSuggestions([]);
        }
      } else {
        setSuggestions([]);
        setShowSuggestions(false);
      }
    };

    fetchSuggestions();
  }, [debouncedSearch]);

  // Click outside to close autocomplete
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowSuggestions(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      setShowSuggestions(false);
      navigate(`/products?search=${encodeURIComponent(searchQuery.trim())}`);
    }
  };

  const toggleLanguage = () => {
    const newLang = i18n.language.startsWith('en') ? 'az' : 'en';
    i18n.changeLanguage(newLang);
  };

  return (
    <header className="sticky top-0 z-50 bg-white/90 dark:bg-zinc-950/90 backdrop-blur-md border-b border-zinc-200 dark:border-zinc-800 transition-colors">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-18 gap-4">
          
          {/* Brand Logo - Vibrant Gradient */}
          <Link to="/" className="flex items-center gap-2.5 flex-shrink-0 group">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-indigo-600 via-purple-600 to-pink-500 flex items-center justify-center text-white transition-transform group-hover:scale-105 shadow-md shadow-indigo-500/25">
              <ShoppingBag className="w-5 h-5" />
            </div>
            <div className="flex flex-col">
              <span className="font-black text-xl tracking-wider bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 bg-clip-text text-transparent dark:from-indigo-400 dark:via-purple-400 dark:to-pink-400 uppercase font-sans">
                ATELIER
              </span>
              <span className="text-[9px] font-bold tracking-widest text-indigo-500 dark:text-indigo-400 uppercase -mt-1">
                E-COMMERCE
              </span>
            </div>
          </Link>

          {/* Search bar with autocomplete */}
          <div ref={searchRef} className="hidden md:flex flex-1 max-w-md lg:max-w-lg relative mx-4">
            <form onSubmit={handleSearchSubmit} className="w-full">
              <div className="relative">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onFocus={() => suggestions.length > 0 && setShowSuggestions(true)}
                  placeholder={t('nav.searchPlaceholder')}
                  className="w-full pl-10 pr-4 py-2.5 text-sm rounded-full border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900 text-zinc-900 dark:text-zinc-100 placeholder-zinc-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all shadow-inner"
                />
                <Search className="w-4 h-4 text-indigo-500 dark:text-indigo-400 absolute left-3.5 top-3.5" />
              </div>
            </form>

            {/* Autocomplete Dropdown */}
            {showSuggestions && suggestions.length > 0 && (
              <div className="absolute top-full left-0 right-0 mt-2 bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl border border-zinc-200 dark:border-zinc-800 overflow-hidden z-50">
                <div className="px-3 py-2 bg-zinc-50 dark:bg-zinc-950/60 border-b border-zinc-100 dark:border-zinc-800 flex items-center justify-between text-[10px] font-black uppercase tracking-wider text-zinc-400">
                  <span>Suggested Products</span>
                  <span>{suggestions.length} items</span>
                </div>
                <ul className="py-1 divide-y divide-zinc-100 dark:divide-zinc-800/50 max-h-80 overflow-y-auto">
                  {suggestions.map((item) => (
                    <li key={item.id}>
                      <button
                        type="button"
                        onClick={() => {
                          setSearchQuery('');
                          setShowSuggestions(false);
                          navigate(`/products/${item.slug}`);
                        }}
                        className="w-full text-left px-3.5 py-2.5 text-xs text-zinc-700 dark:text-zinc-200 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 flex items-center gap-3 transition-colors group"
                      >
                        {item.primaryImageUrl ? (
                          <img
                            src={item.primaryImageUrl}
                            alt={item.name}
                            className="w-9 h-9 object-cover rounded-lg border border-zinc-200 dark:border-zinc-800 flex-shrink-0"
                          />
                        ) : (
                          <div className="w-9 h-9 rounded-lg bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center flex-shrink-0 text-zinc-400">
                            <Search className="w-4 h-4" />
                          </div>
                        )}
                        <div className="flex-1 min-w-0">
                          <p className="font-bold text-xs truncate group-hover:text-indigo-600 dark:group-hover:text-indigo-400 text-zinc-900 dark:text-zinc-100">
                            {item.name}
                          </p>
                          {item.categoryName && (
                            <span className="text-[10px] text-zinc-400 block font-light">
                              {item.categoryName}
                            </span>
                          )}
                        </div>
                        {item.basePrice != null && (
                          <span className="font-black text-xs text-indigo-600 dark:text-indigo-400">
                            ${item.basePrice.toFixed(2)}
                          </span>
                        )}
                      </button>
                    </li>
                  ))}
                </ul>
                <div className="p-2.5 bg-zinc-50 dark:bg-zinc-950/60 border-t border-zinc-100 dark:border-zinc-800 text-center">
                  <button
                    type="button"
                    onClick={() => {
                      setShowSuggestions(false);
                      navigate(`/products?search=${encodeURIComponent(searchQuery.trim())}`);
                    }}
                    className="text-[11px] font-black uppercase tracking-wider text-indigo-600 dark:text-indigo-400 hover:underline"
                  >
                    View all matching results for "{searchQuery}" →
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Navigation Links (Desktop) */}
          <nav className="hidden lg:flex items-center gap-7 text-sm font-semibold tracking-wide text-zinc-600 dark:text-zinc-300">
            <Link to="/" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
              {t('nav.home')}
            </Link>
            <Link to="/products" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
              {t('nav.products')}
            </Link>
            <Link to="/categories" className="hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors">
              {t('nav.categories')}
            </Link>
          </nav>

          {/* Action Buttons: Theme, Language, Wishlist, Cart, Auth */}
          <div className="flex items-center gap-2 sm:gap-3">
            
            {/* Colorful Theme Toggle Button */}
            <button
              onClick={toggleTheme}
              title={`Switch to ${theme === 'dark' ? 'Light' : 'Dark'} theme`}
              className="p-2.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-amber-50 dark:hover:bg-indigo-950/40 hover:border-amber-300 dark:hover:border-indigo-700 transition-all shadow-sm"
            >
              {theme === 'dark' ? (
                <Sun className="w-4 h-4 text-amber-400 hover:rotate-45 transition-transform" />
              ) : (
                <Moon className="w-4 h-4 text-indigo-600 hover:-rotate-12 transition-transform" />
              )}
            </button>

            {/* Language Switcher */}
            <button
              onClick={toggleLanguage}
              title="Switch Language"
              className="px-2.5 py-1.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 hover:border-indigo-300 dark:hover:border-indigo-700 transition-colors flex items-center gap-1.5 text-xs font-bold"
            >
              <Globe className="w-3.5 h-3.5 text-indigo-500" />
              <span className="uppercase">{i18n.language.substring(0, 2)}</span>
            </button>

            {/* Wishlist Link */}
            <Link
              to="/wishlist"
              className="relative p-2.5 rounded-full border border-zinc-200 dark:border-zinc-800 text-zinc-700 dark:text-zinc-300 hover:bg-rose-50 dark:hover:bg-rose-950/30 hover:border-rose-300 dark:hover:border-rose-800 transition-colors shadow-sm"
              title={t('nav.wishlist')}
            >
              <Heart className="w-4 h-4 hover:text-rose-500 transition-colors" />
              {wishlistCount > 0 && (
                <span className="absolute -top-1 -right-1 w-4 h-4 bg-rose-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center shadow-sm shadow-rose-500/50">
                  {wishlistCount}
                </span>
              )}
            </Link>

            {/* Cart Link with Gradient & Amber Badge */}
            <Link
              to="/cart"
              className="relative p-2.5 rounded-full bg-gradient-to-r from-indigo-600 via-purple-600 to-indigo-700 text-white hover:opacity-95 hover:shadow-lg hover:shadow-indigo-500/30 transition-all shadow-md transform hover:scale-105 active:scale-95"
              title={t('nav.cart')}
            >
              <ShoppingCart className="w-4 h-4" />
              {itemsCount > 0 && (
                <span className="absolute -top-1 -right-1 min-w-[16px] h-4 px-1 bg-amber-400 text-zinc-950 text-[10px] font-black rounded-full flex items-center justify-center shadow-md">
                  {itemsCount}
                </span>
              )}
            </Link>

            {/* Auth Menu */}
            {isAuthenticated ? (
              <div className="relative">
                <button
                  onClick={() => setProfileDropdownOpen((prev) => !prev)}
                  className="flex items-center gap-2 p-1.5 rounded-full hover:bg-zinc-100 dark:hover:bg-zinc-900 transition-colors"
                >
                  <div className="w-8 h-8 rounded-full bg-gradient-to-tr from-indigo-500 to-purple-600 text-white font-bold flex items-center justify-center text-xs shadow-sm">
                    {user?.firstName?.charAt(0) || <User className="w-4 h-4" />}
                  </div>
                </button>

                {profileDropdownOpen && (
                  <div className="absolute right-0 mt-2 w-56 bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl border border-zinc-200 dark:border-zinc-800 py-2 z-50 animate-in fade-in slide-in-from-top-2 duration-150">
                    <div className="px-4 py-2.5 border-b border-zinc-100 dark:border-zinc-800">
                      <p className="text-[11px] font-semibold text-zinc-400 uppercase tracking-wider">Signed in as</p>
                      <p className="text-sm font-bold text-zinc-900 dark:text-zinc-100 truncate">
                        {user?.firstName} {user?.lastName}
                      </p>
                      <p className="text-xs text-zinc-500 dark:text-zinc-400 truncate">{user?.email}</p>
                    </div>

                    <Link
                      to="/my-orders"
                      onClick={() => setProfileDropdownOpen(false)}
                      className="px-4 py-2.5 text-sm text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-800 flex items-center gap-2.5 transition-colors"
                    >
                      <Package className="w-4 h-4" />
                      <span>{t('nav.myOrders')}</span>
                    </Link>

                    {user?.roles?.includes('SuperAdmin') && (
                      <Link
                        to="/admin"
                        onClick={() => setProfileDropdownOpen(false)}
                        className="px-4 py-2.5 text-sm font-semibold text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 flex items-center gap-2.5 transition-colors"
                      >
                        <ShieldCheck className="w-4 h-4 text-indigo-500" />
                        <span>{t('nav.admin')}</span>
                      </Link>
                    )}

                    <div className="border-t border-zinc-100 dark:border-zinc-800 my-1" />

                    <button
                      onClick={() => {
                        setProfileDropdownOpen(false);
                        logout();
                      }}
                      className="w-full text-left px-4 py-2.5 text-sm text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/30 flex items-center gap-2.5 transition-colors"
                    >
                      <LogOut className="w-4 h-4" />
                      <span>{t('nav.logout')}</span>
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <div className="hidden sm:flex items-center gap-2">
                <Link
                  to="/login"
                  className="px-4 py-2 text-xs font-bold uppercase tracking-wider text-zinc-700 dark:text-zinc-300 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                >
                  {t('nav.login')}
                </Link>
                <Link
                  to="/register"
                  className="px-4 py-2 rounded-full text-xs font-bold uppercase tracking-wider bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white hover:opacity-95 shadow-md shadow-indigo-500/25 transition-transform hover:scale-105"
                >
                  {t('nav.register')}
                </Link>
              </div>
            )}

            {/* Mobile Menu Toggle */}
            <button
              onClick={() => setMobileMenuOpen((prev) => !prev)}
              className="lg:hidden p-2 rounded-lg text-zinc-700 dark:text-zinc-300 hover:bg-zinc-100 dark:hover:bg-zinc-900"
            >
              {mobileMenuOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </button>
          </div>
        </div>

        {/* Mobile Search Bar */}
        <div className="md:hidden pb-3 relative">
          <form onSubmit={handleSearchSubmit}>
            <div className="relative">
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onFocus={() => suggestions.length > 0 && setShowSuggestions(true)}
                placeholder={t('nav.searchPlaceholder')}
                className="w-full pl-10 pr-4 py-2 text-sm rounded-full border border-zinc-200 dark:border-zinc-800 bg-zinc-50 dark:bg-zinc-900 text-zinc-900 dark:text-zinc-100 placeholder-zinc-400 focus:outline-none"
              />
              <Search className="w-4 h-4 text-zinc-400 absolute left-3.5 top-3" />
            </div>
          </form>

          {showSuggestions && suggestions.length > 0 && (
            <div className="absolute top-full left-0 right-0 mt-1 bg-white dark:bg-zinc-900 rounded-2xl shadow-2xl border border-zinc-200 dark:border-zinc-800 overflow-hidden z-50">
              <ul className="py-1 divide-y divide-zinc-100 dark:divide-zinc-800/50 max-h-60 overflow-y-auto">
                {suggestions.map((item) => (
                  <li key={item.id}>
                    <button
                      type="button"
                      onClick={() => {
                        setSearchQuery('');
                        setShowSuggestions(false);
                        navigate(`/products/${item.slug}`);
                      }}
                      className="w-full text-left px-3.5 py-2 text-xs text-zinc-700 dark:text-zinc-200 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 flex items-center gap-2.5 transition-colors"
                    >
                      {item.primaryImageUrl && (
                        <img
                          src={item.primaryImageUrl}
                          alt={item.name}
                          className="w-7 h-7 object-cover rounded-md flex-shrink-0"
                        />
                      )}
                      <span className="font-semibold truncate flex-1">{item.name}</span>
                      {item.basePrice != null && (
                        <span className="font-bold text-indigo-600 dark:text-indigo-400">
                          ${item.basePrice.toFixed(2)}
                        </span>
                      )}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </div>
      </div>

      {/* Mobile Drawer Menu */}
      {mobileMenuOpen && (
        <div className="lg:hidden border-t border-zinc-200 dark:border-zinc-800 bg-white dark:bg-zinc-950 px-4 py-6 space-y-4">
          <nav className="flex flex-col space-y-3 font-semibold text-zinc-800 dark:text-zinc-200">
            <Link
              to="/"
              onClick={() => setMobileMenuOpen(false)}
              className="px-3 py-2 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-900"
            >
              {t('nav.home')}
            </Link>
            <Link
              to="/products"
              onClick={() => setMobileMenuOpen(false)}
              className="px-3 py-2 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-900"
            >
              {t('nav.products')}
            </Link>
            <Link
              to="/categories"
              onClick={() => setMobileMenuOpen(false)}
              className="px-3 py-2 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-900"
            >
              {t('nav.categories')}
            </Link>
            <Link
              to="/wishlist"
              onClick={() => setMobileMenuOpen(false)}
              className="px-3 py-2 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-900"
            >
              {t('nav.wishlist')} ({wishlistCount})
            </Link>
            <Link
              to="/cart"
              onClick={() => setMobileMenuOpen(false)}
              className="px-3 py-2 rounded-lg hover:bg-zinc-100 dark:hover:bg-zinc-900"
            >
              {t('nav.cart')} ({itemsCount})
            </Link>
          </nav>

          {!isAuthenticated && (
            <div className="pt-4 border-t border-zinc-200 dark:border-zinc-800 flex flex-col gap-2">
              <Link
                to="/login"
                onClick={() => setMobileMenuOpen(false)}
                className="w-full text-center py-2.5 rounded-full text-sm font-bold border border-zinc-200 dark:border-zinc-800 text-zinc-800 dark:text-zinc-200 hover:text-indigo-600 dark:hover:text-indigo-400"
              >
                {t('nav.login')}
              </Link>
              <Link
                to="/register"
                onClick={() => setMobileMenuOpen(false)}
                className="w-full text-center py-2.5 rounded-full text-sm font-bold bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 text-white shadow-md shadow-indigo-500/25"
              >
                {t('nav.register')}
              </Link>
            </div>
          )}
        </div>
      )}
    </header>
  );
};
