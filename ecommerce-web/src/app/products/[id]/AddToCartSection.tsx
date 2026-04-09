"use client";

import { useState } from "react";
import { ShoppingCart, Check, Minus, Plus } from "lucide-react";
import { Product, CartItem } from "@/lib/types";
import { useCartStore } from "@/store/cartStore";

interface AddToCartSectionProps {
  product: Product;
}

export default function AddToCartSection({ product }: AddToCartSectionProps) {
  const [selectedVariant, setSelectedVariant] = useState(
    product.variants?.[0] || null,
  );
  const [quantity, setQuantity] = useState(1);
  const [added, setAdded] = useState(false);
  const addItem = useCartStore((state) => state.addItem);

  const currentPrice = product.hasVariants
    ? (selectedVariant?.price ?? 0)
    : (product.price ?? 0);

  const comparePrice = product.hasVariants
    ? selectedVariant?.compareAtPrice
    : product.compareAtPrice;

  const inStock = product.hasVariants
    ? (selectedVariant?.inventory?.quantity ?? 0) > 0
    : (product.inventory?.quantity ?? 0) > 0;

  const handleAddToCart = () => {
    const cartItem: CartItem = {
      productId: product.id,
      productName: product.name,
      sku: product.sku,
      variantSku: selectedVariant?.sku,
      variantName: selectedVariant?.name,
      price: currentPrice,
      quantity,
      image: selectedVariant?.image || product.images?.[0]?.url,
      categoryId: product.categoryId,
    };
    addItem(cartItem);
    setAdded(true);
    setTimeout(() => setAdded(false), 2000);
  };

  return (
    <div>
      {/* Price Display */}
      <div className="flex items-baseline space-x-3 mb-6">
        <span className="text-3xl font-bold text-gray-900">
          ${currentPrice.toFixed(2)}
        </span>
        {comparePrice && comparePrice > currentPrice && (
          <span className="text-lg text-gray-400 line-through">
            ${comparePrice.toFixed(2)}
          </span>
        )}
      </div>

      {/* Variant Selection */}
      {product.hasVariants && product.variants && (
        <div className="mb-6">
          <h3 className="text-sm font-medium text-gray-900 mb-3">Options</h3>
          <div className="flex flex-wrap gap-2">
            {product.variants.map((variant) => (
              <button
                key={variant.variantId}
                onClick={() => setSelectedVariant(variant)}
                className={`px-4 py-2 rounded-lg border text-sm font-medium transition-colors ${
                  selectedVariant?.variantId === variant.variantId
                    ? "border-blue-600 bg-blue-50 text-blue-600"
                    : "border-gray-300 text-gray-700 hover:border-gray-400"
                } ${!variant.isActive ? "opacity-50 cursor-not-allowed" : ""}`}
                disabled={!variant.isActive}
              >
                {variant.name}
                {variant.inventory.quantity === 0 && " (Out of Stock)"}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Quantity Selector */}
      <div className="flex items-center space-x-4 mb-6">
        <span className="text-sm font-medium text-gray-900">Quantity</span>
        <div className="flex items-center border border-gray-300 rounded-lg">
          <button
            onClick={() => setQuantity(Math.max(1, quantity - 1))}
            className="px-3 py-2 hover:bg-gray-100 transition-colors"
          >
            <Minus size={16} />
          </button>
          <span className="px-4 py-2 text-sm font-medium border-x border-gray-300">
            {quantity}
          </span>
          <button
            onClick={() => setQuantity(quantity + 1)}
            className="px-3 py-2 hover:bg-gray-100 transition-colors"
          >
            <Plus size={16} />
          </button>
        </div>
      </div>

      {/* Stock Status */}
      <p
        className={`text-sm mb-4 ${inStock ? "text-green-600" : "text-red-600"}`}
      >
        {inStock ? "✓ In Stock" : "✗ Out of Stock"}
      </p>

      {/* Add to Cart Button */}
      <button
        onClick={handleAddToCart}
        disabled={!inStock}
        className={`w-full flex items-center justify-center space-x-2 py-3 px-6 rounded-lg text-lg font-semibold transition-colors ${
          added
            ? "bg-green-600 text-white"
            : inStock
              ? "bg-blue-600 hover:bg-blue-700 text-white"
              : "bg-gray-300 text-gray-500 cursor-not-allowed"
        }`}
      >
        {added ? (
          <>
            <Check size={20} />
            <span>Added to Cart!</span>
          </>
        ) : (
          <>
            <ShoppingCart size={20} />
            <span>Add to Cart — ${(currentPrice * quantity).toFixed(2)}</span>
          </>
        )}
      </button>
    </div>
  );
}
