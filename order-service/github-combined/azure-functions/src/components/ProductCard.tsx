"use client";

import Link from "next/link";
import Image from "next/image";
import { ShoppingCart } from "lucide-react";
import { Product, CartItem } from "@/lib/types";
import { useCartStore } from "@/store/cartStore";

interface ProductCardProps {
  product: Product;
}

export default function ProductCard({ product }: ProductCardProps) {
  const addItem = useCartStore((state) => state.addItem);

  const primaryImage =
    product.images?.find((img) => img.isPrimary) || product.images?.[0];
  const displayPrice = product.hasVariants
    ? product.priceRange
      ? `$${product.priceRange.min.toFixed(2)} - $${product.priceRange.max.toFixed(2)}`
      : "Price varies"
    : `$${(product.price ?? 0).toFixed(2)}`;

  const comparePrice =
    !product.hasVariants && product.compareAtPrice
      ? `$${product.compareAtPrice.toFixed(2)}`
      : null;

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault(); // Don't navigate to product detail
    e.stopPropagation();

    // For variant products, navigate to detail page instead
    if (product.hasVariants) {
      window.location.href = `/products/${product.id}?categoryId=${product.categoryId}`;
      return;
    }

    const cartItem: CartItem = {
      productId: product.id,
      productName: product.name,
      sku: product.sku,
      price: product.price ?? 0,
      quantity: 1,
      image: primaryImage?.url,
      categoryId: product.categoryId,
    };
    addItem(cartItem);
  };

  return (
    <Link
      href={`/products/${product.id}?categoryId=${product.categoryId}`}
      className="group bg-white rounded-lg shadow-sm hover:shadow-md transition-shadow overflow-hidden border border-gray-100"
    >
      {/* Product Image */}
      <div className="relative aspect-square bg-gray-100 overflow-hidden">
        {primaryImage ? (
          <img
            src={primaryImage.url}
            alt={primaryImage.altText || product.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-gray-400">
            <span className="text-6xl">🪑</span>
          </div>
        )}

        {/* Featured Badge */}
        {product.isFeatured && (
          <span className="absolute top-2 left-2 bg-blue-600 text-white text-xs px-2 py-1 rounded-full font-medium">
            Featured
          </span>
        )}

        {/* Sale Badge */}
        {comparePrice && (
          <span className="absolute top-2 right-2 bg-red-500 text-white text-xs px-2 py-1 rounded-full font-medium">
            Sale
          </span>
        )}
      </div>

      {/* Product Info */}
      <div className="p-4">
        <p className="text-xs text-gray-500 uppercase tracking-wide mb-1">
          {product.categoryName || product.categoryId}
        </p>
        <h3 className="text-sm font-semibold text-gray-900 mb-2 line-clamp-2 group-hover:text-blue-600 transition-colors">
          {product.name}
        </h3>

        {/* Price */}
        <div className="flex items-center space-x-2 mb-3">
          <span className="text-lg font-bold text-gray-900">
            {displayPrice}
          </span>
          {comparePrice && (
            <span className="text-sm text-gray-400 line-through">
              {comparePrice}
            </span>
          )}
        </div>

        {/* Add to Cart Button */}
        <button
          onClick={handleAddToCart}
          className="w-full flex items-center justify-center space-x-2 bg-blue-600 hover:bg-blue-700 text-white py-2 px-4 rounded-lg transition-colors text-sm font-medium"
        >
          <ShoppingCart size={16} />
          <span>{product.hasVariants ? "View Options" : "Add to Cart"}</span>
        </button>
      </div>
    </Link>
  );
}
