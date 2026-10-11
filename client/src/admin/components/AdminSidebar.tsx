import React, { useRef, useEffect } from 'react';
import { NavLink, Link } from 'react-router-dom';
import {
  LayoutDashboard,
  Package,
  Layers,
  Tag,
  Trash2,
  ShoppingCart,
  Users,
  ShieldCheck,
  Ticket,
  Star,
  Image,
  Truck,
  RotateCcw,
  FileText,
  Activity,
  X,
  ShoppingBag,
} from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

export const AdminSidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const { user } = useAuth();
  const navRef = useRef<HTMLElement>(null);
  const isAdmin = user?.roles?.some((r) => r.toLowerCase() === 'admin' || r.toLowerCase() === 'superadmin');

  useEffect(() => {
    const saved = sessionStorage.getItem('admin_sidebar_scroll');
    if (saved && navRef.current) {
      navRef.current.scrollTop = parseInt(saved, 10);
    }
  }, []);

  const menuItems = [
    { title: 'Dashboard', path: '/admin', icon: LayoutDashboard },
    { title: 'Products', path: '/admin/products', icon: Package, permission: 'product.view' },
    { title: 'Categories', path: '/admin/categories', icon: Layers, permission: 'category.view' },
    { title: 'Brands', path: '/admin/brands', icon: Tag, permission: 'brand.view' },
    { title: 'Deleted Items', path: '/admin/deleted', icon: Trash2, permission: 'trash.manage' },
    { title: 'Orders', path: '/admin/orders', icon: ShoppingCart, permission: 'order.view' },
    { title: 'Users', path: '/admin/users', icon: Users, permission: 'user.view' },
    { title: 'Roles & Access', path: '/admin/roles', icon: ShieldCheck, permission: 'role.manage' },
    { title: 'Coupons', path: '/admin/coupons', icon: Ticket, permission: 'coupon.view' },
    { title: 'Reviews', path: '/admin/reviews', icon: Star, permission: 'review.moderate' },
    { title: 'Banners', path: '/admin/banners', icon: Image, permission: 'banner.manage' },
    { title: 'Shipping', path: '/admin/shipping', icon: Truck, permission: 'shipping.manage' },
    { title: 'Returns', path: '/admin/returns', icon: RotateCcw, permission: 'return.manage' },
    { title: 'Audit Logs', path: '/admin/audit', icon: FileText, permission: 'audit.view' },
    { title: 'Stock Movements', path: '/admin/stock', icon: Activity, permission: 'stock.view' },
  ];

  const filteredItems = menuItems.filter((item) => {
    if (isAdmin) return true;
    if (!item.permission) return true;
    return user?.permissions?.includes(item.permission);
  });

  return (
    <>
      {/* Mobile Backdrop */}
      {isOpen && (
        <div
          onClick={onClose}
          className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm lg:hidden transition-opacity"
        />
      )}

      {/* Sidebar Content */}
      <aside
        className={`fixed top-0 bottom-0 left-0 z-50 w-64 bg-white dark:bg-zinc-950 border-r border-zinc-200 dark:border-zinc-800 flex flex-col transition-transform duration-300 ease-in-out lg:translate-x-0 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {/* Sidebar Header */}
        <div className="h-18 px-6 flex items-center justify-between border-b border-zinc-200 dark:border-zinc-800">
          <Link to="/" className="flex items-center gap-2.5">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-tr from-indigo-600 via-purple-600 to-pink-500 text-white flex items-center justify-center shadow-md shadow-indigo-500/25">
              <ShoppingBag className="w-4 h-4" />
            </div>
            <div className="flex flex-col">
              <span className="font-black text-base tracking-wider uppercase bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                ATELIER
              </span>
              <span className="text-[9px] font-bold text-indigo-500 dark:text-indigo-400 uppercase -mt-1 tracking-widest">
                ADMIN CONSOLE
              </span>
            </div>
          </Link>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-zinc-400 hover:text-indigo-600 lg:hidden"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Navigation Items */}
        <nav
          ref={navRef}
          onScroll={(e) => sessionStorage.setItem('admin_sidebar_scroll', String(e.currentTarget.scrollTop))}
          className="flex-1 overflow-y-auto px-4 py-5 space-y-1"
        >
          {filteredItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/admin'}
              onClick={() => onClose()}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-wider transition-all ${
                  isActive
                    ? 'bg-gradient-to-r from-indigo-600 via-purple-600 to-indigo-700 text-white shadow-md shadow-indigo-500/25'
                    : 'text-zinc-600 dark:text-zinc-400 hover:bg-indigo-50 dark:hover:bg-indigo-950/40 hover:text-indigo-600 dark:hover:text-indigo-400'
                }`
              }
            >
              <item.icon className="w-4 h-4" />
              <span>{item.title}</span>
            </NavLink>
          ))}
        </nav>

        {/* Footer Info */}
        <div className="p-4 border-t border-zinc-200 dark:border-zinc-800">
          <div className="p-3 rounded-2xl bg-indigo-50/50 dark:bg-indigo-950/30 border border-indigo-100 dark:border-indigo-900/50">
            <p className="text-[10px] font-bold uppercase tracking-widest text-indigo-500 dark:text-indigo-400">Security State</p>
            <p className="text-xs font-black text-zinc-900 dark:text-zinc-100 mt-0.5">Role: {user?.roles?.[0] || 'Admin'}</p>
          </div>
        </div>
      </aside>
    </>
  );
};
