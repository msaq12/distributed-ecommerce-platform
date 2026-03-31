"use client";

import { Minus, Plus, Trash2 } from "lucide-react";
import { CartItem as CartItemType } from "@/lib/types";
import { useCartStore } from "@/store/cartStore";

interface CartItemProps {
  item: CartItemType;
}

export default function CartItem({ item }: CartItemProps) {
  const { updateQuantity, removeItem } = useCartStore();

  return (
    <div className="flex items-center py-4 border-b border-gray-200">
      {/* Product Image */}
      <div className="w-20 h-20 bg-gray-100 rounded-lg overflow-hidden flex-shrink-0">
        {item.image ? (
          <img
            src={item.image}
            alt={item.productName}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-gray-400">
            <span className="text-3xl">🪑</span>
          </div>
        )}
      </div>

      {/* Product Details */}
      <div className="ml-4 flex-1">
        <h3 className="text-sm font-semibold text-gray-900">
          {item.productName}
        </h3>
        {item.variantName && (
          <p className="text-xs text-gray-500 mt-1">{item.variantName}</p>
        )}
        <p className="text-xs text-gray-400 mt-1">
          SKU: {item.variantSku || item.sku}
        </p>
        <p className="text-sm font-bold text-gray-900 mt-1">
          ${item.price.toFixed(2)}
        </p>
      </div>

      {/* Quantity Controls */}
      <div className="flex items-center space-x-2">
        <button
          onClick={() =>
            updateQuantity(item.productId, item.quantity - 1, item.variantSku)
          }
          className="p-1 rounded-md border border-gray-300 hover:bg-gray-100 transition-colors"
        >
          <Minus size={16} />
        </button>
        <span className="w-8 text-center text-sm font-medium">
          {item.quantity}
        </span>
        <button
          onClick={() =>
            updateQuantity(item.productId, item.quantity + 1, item.variantSku)
          }
          className="p-1 rounded-md border border-gray-300 hover:bg-gray-100 transition-colors"
        >
          <Plus size={16} />
        </button>
      </div>

      {/* Line Total */}
      <div className="ml-4 text-right w-24">
        <p className="text-sm font-bold text-gray-900">
          ${(item.price * item.quantity).toFixed(2)}
        </p>
      </div>

      {/* Remove Button */}
      <button
        onClick={() => removeItem(item.productId, item.variantSku)}
        className="ml-4 p-2 text-gray-400 hover:text-red-500 transition-colors"
      >
        <Trash2 size={18} />
      </button>
    </div>
  );
}
