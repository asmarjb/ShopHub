import React, { createContext, useContext, useEffect, useState } from 'react';
import { Cart, CartItem } from '../types';
import { cartService } from '../services/cartService';
import { useAuth } from './AuthContext';
import toast from 'react-hot-toast';

interface CartContextType {
  cart: Cart | null;
  itemsCount: number;
  isLoading: boolean;
  addToCart: (productId: string, variantId?: string, quantity?: number) => Promise<void>;
  updateQuantity: (itemId: string, quantity: number) => Promise<void>;
  removeItem: (itemId: string) => Promise<void>;
  clearCart: () => Promise<void>;
  refreshCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const [cart, setCart] = useState<Cart | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const refreshCart = async () => {
    try {
      const data = await cartService.getCart();
      setCart(data);
    } catch {
      setCart(null);
    }
  };

  useEffect(() => {
    refreshCart();
  }, [isAuthenticated]);

  const addToCart = async (productId: string, variantId?: string, quantity = 1) => {
    setIsLoading(true);
    try {
      const updated = await cartService.addItem(productId, variantId, quantity);
      setCart(updated);
      toast.success('Item added to cart!');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to add item to cart.');
    } finally {
      setIsLoading(false);
    }
  };

  const updateQuantity = async (itemId: string, quantity: number) => {
    try {
      if (quantity <= 0) {
        await removeItem(itemId);
        return;
      }
      const updated = await cartService.updateQuantity(itemId, quantity);
      setCart(updated);
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Say yenilənərkən xəta baş verdi.');
    }
  };

  const removeItem = async (itemId: string) => {
    try {
      const updated = await cartService.removeItem(itemId);
      setCart(updated);
      toast.success('Məhsul səbətdən silindi.');
    } catch {
      toast.error('Silinərkən xəta baş verdi.');
    }
  };

  const clearCart = async () => {
    try {
      await cartService.clearCart();
      setCart(null);
    } catch {
      toast.error('Səbət təmizlənərkən xəta baş verdi.');
    }
  };

  const itemsCount = cart?.items?.reduce((sum: number, item: CartItem) => sum + item.quantity, 0) || 0;

  return (
    <CartContext.Provider
      value={{
        cart,
        itemsCount,
        isLoading,
        addToCart,
        updateQuantity,
        removeItem,
        clearCart,
        refreshCart,
      }}
    >
      {children}
    </CartContext.Provider>
  );
};

export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) throw new Error('useCart must be used within CartProvider');
  return context;
};
