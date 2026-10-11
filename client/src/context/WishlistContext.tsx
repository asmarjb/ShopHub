import React, { createContext, useContext, useEffect, useState } from 'react';
import { api } from '../services/api';
import { useAuth } from './AuthContext';
import toast from 'react-hot-toast';

interface WishlistContextType {
  wishlistIds: string[];
  isInWishlist: (productId: string) => boolean;
  toggleWishlist: (productId: string) => Promise<void>;
  count: number;
}

const WishlistContext = createContext<WishlistContextType | undefined>(undefined);

export const WishlistProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const [wishlistIds, setWishlistIds] = useState<string[]>([]);

  useEffect(() => {
    if (!isAuthenticated) {
      setWishlistIds([]);
      return;
    }

    const fetchWishlist = async () => {
      try {
        const res = await api.get('/wishlist');
        const items = Array.isArray(res.data) ? res.data : (res.data?.data || []);
        setWishlistIds(items.map((i: any) => i.productId || i.id));
      } catch {
        // xəta olduqda susdurulur
      }
    };

    fetchWishlist();
  }, [isAuthenticated]);

  const isInWishlist = (productId: string) => wishlistIds.includes(productId);

  const toggleWishlist = async (productId: string) => {
    if (!isAuthenticated) {
      toast.error('İstək siyahısını istifadə etmək üçün daxil olun.');
      return;
    }

    const exists = isInWishlist(productId);
    try {
      if (exists) {
        await api.delete(`/wishlist/${productId}`);
        setWishlistIds((prev) => prev.filter((id) => id !== productId));
        toast.success('Məhsul istək siyahısından çıxarıldı.');
      } else {
        await api.post(`/wishlist/${productId}`);
        setWishlistIds((prev) => [...prev, productId]);
        toast.success('Məhsul istək siyahısına əlavə olundu.');
      }
    } catch {
      toast.error('Əməliyyat zamanı xəta baş verdi.');
    }
  };

  return (
    <WishlistContext.Provider
      value={{
        wishlistIds,
        isInWishlist,
        toggleWishlist,
        count: wishlistIds.length,
      }}
    >
      {children}
    </WishlistContext.Provider>
  );
};

export const useWishlist = () => {
  const context = useContext(WishlistContext);
  if (!context) throw new Error('useWishlist must be used within WishlistProvider');
  return context;
};
