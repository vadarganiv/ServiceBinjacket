'use client';

import { createContext, useContext, useEffect, useReducer, type ReactNode } from 'react';
import type { CartItem } from '@/lib/types';

const STORAGE_KEY = 'sb_cart';

type Action =
  | { type: 'ADD'; item: CartItem }
  | { type: 'REMOVE'; productId: number }
  | { type: 'UPDATE_QTY'; productId: number; quantity: number }
  | { type: 'CLEAR' }
  | { type: 'HYDRATE'; items: CartItem[] };

function reducer(state: CartItem[], action: Action): CartItem[] {
  switch (action.type) {
    case 'HYDRATE':
      return action.items;
    case 'ADD': {
      const existing = state.find(i => i.productId === action.item.productId);
      if (existing) {
        return state.map(i =>
          i.productId === action.item.productId
            ? { ...i, quantity: i.quantity + action.item.quantity }
            : i
        );
      }
      return [...state, action.item];
    }
    case 'REMOVE':
      return state.filter(i => i.productId !== action.productId);
    case 'UPDATE_QTY':
      if (action.quantity <= 0)
        return state.filter(i => i.productId !== action.productId);
      return state.map(i =>
        i.productId === action.productId ? { ...i, quantity: action.quantity } : i
      );
    case 'CLEAR':
      return [];
    default:
      return state;
  }
}

interface CartContextValue {
  items: CartItem[];
  totalCount: number;
  subtotal: number;
  addItem: (item: CartItem) => void;
  removeItem: (productId: number) => void;
  updateQuantity: (productId: number, quantity: number) => void;
  clearCart: () => void;
}

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, dispatch] = useReducer(reducer, []);

  useEffect(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) dispatch({ type: 'HYDRATE', items: JSON.parse(stored) });
    } catch {
      // ignore malformed storage
    }
  }, []);

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
      // ignore storage quota errors
    }
  }, [items]);

  const totalCount = items.reduce((sum, i) => sum + i.quantity, 0);
  const subtotal = items.reduce((sum, i) => sum + i.price * i.quantity, 0);

  return (
    <CartContext.Provider value={{
      items,
      totalCount,
      subtotal,
      addItem: (item) => dispatch({ type: 'ADD', item }),
      removeItem: (productId) => dispatch({ type: 'REMOVE', productId }),
      updateQuantity: (productId, quantity) => dispatch({ type: 'UPDATE_QTY', productId, quantity }),
      clearCart: () => dispatch({ type: 'CLEAR' }),
    }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart(): CartContextValue {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart must be used inside CartProvider');
  return ctx;
}
