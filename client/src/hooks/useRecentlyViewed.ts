import { useState, useEffect } from 'react';
import { Product } from '../types';

const STORAGE_KEY = 'recently_viewed_products';

export function useRecentlyViewed() {
  const [items, setItems] = useState<Product[]>([]);

  useEffect(() => {
    try {
      const data = localStorage.getItem(STORAGE_KEY);
      if (data) {
        setItems(JSON.parse(data));
      }
    } catch {
      setItems([]);
    }
  }, []);

  const addProduct = (product: Product) => {
    setItems((prev) => {
      const filtered = prev.filter((p) => p.id !== product.id);
      const updated = [product, ...filtered].slice(0, 8);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
      return updated;
    });
  };

  return { items, addProduct };
}
