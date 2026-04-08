import { create } from "zustand";
import { CartItem } from "@/lib/types";

interface CartState {
  items: CartItem[];
  addItem: (item: CartItem) => void;
  removeItem: (productId: string, variantSku?: string) => void;
  updateQuantity: (
    productId: string,
    quantity: number,
    variantSku?: string,
  ) => void;
  clearCart: () => void;
  getTotal: () => number;
  getItemCount: () => number;
}

export const useCartStore = create<CartState>((set, get) => ({
  items: [],

  addItem: (item: CartItem) => {
    set((state) => {
      // Check if item already in cart (match by productId + variantSku)
      const existingIndex = state.items.findIndex(
        (i) =>
          i.productId === item.productId && i.variantSku === item.variantSku,
      );

      if (existingIndex >= 0) {
        // Update quantity
        const updatedItems = [...state.items];
        updatedItems[existingIndex] = {
          ...updatedItems[existingIndex],
          quantity: updatedItems[existingIndex].quantity + item.quantity,
        };
        return { items: updatedItems };
      }

      // Add new item
      return { items: [...state.items, item] };
    });
  },

  removeItem: (productId: string, variantSku?: string) => {
    set((state) => ({
      items: state.items.filter(
        (i) => !(i.productId === productId && i.variantSku === variantSku),
      ),
    }));
  },

  updateQuantity: (
    productId: string,
    quantity: number,
    variantSku?: string,
  ) => {
    set((state) => {
      if (quantity <= 0) {
        return {
          items: state.items.filter(
            (i) => !(i.productId === productId && i.variantSku === variantSku),
          ),
        };
      }
      return {
        items: state.items.map((i) =>
          i.productId === productId && i.variantSku === variantSku
            ? { ...i, quantity }
            : i,
        ),
      };
    });
  },

  clearCart: () => set({ items: [] }),

  getTotal: () => {
    return get().items.reduce(
      (total, item) => total + item.price * item.quantity,
      0,
    );
  },

  getItemCount: () => {
    return get().items.reduce((count, item) => count + item.quantity, 0);
  },
}));
