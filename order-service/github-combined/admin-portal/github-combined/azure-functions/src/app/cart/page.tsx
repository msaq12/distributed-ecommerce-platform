"use client";

import Link from "next/link";
import { ShoppingCart, ArrowLeft } from "lucide-react";
import { useCartStore } from "@/store/cartStore";
import CartItem from "@/components/CartItem";

export default function CartPage() {
  const { items, getTotal, clearCart } = useCartStore();
  const subtotal = getTotal();
  const tax = subtotal * 0.08; // 8% tax (matches Order Service)
  const shipping = subtotal >= 100 ? 0 : 10.0; // Free shipping over $100
  const total = subtotal + tax + shipping;

  if (items.length === 0) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center">
        <ShoppingCart size={64} className="mx-auto text-gray-300 mb-6" />
        <h1 className="text-2xl font-bold text-gray-900 mb-3">
          Your Cart is Empty
        </h1>
        <p className="text-gray-500 mb-8">
          Looks like you haven't added any items to your cart yet.
        </p>
        <Link
          href="/products"
          className="inline-flex items-center space-x-2 bg-blue-600 text-white px-6 py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
        >
          <ArrowLeft size={20} />
          <span>Continue Shopping</span>
        </Link>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
      {/* Breadcrumb */}
      <nav className="text-sm text-gray-500 mb-6">
        <Link href="/" className="hover:text-blue-600">
          Home
        </Link>
        <span className="mx-2">/</span>
        <span className="text-gray-900">Shopping Cart</span>
      </nav>

      <h1 className="text-2xl font-bold text-gray-900 mb-8">
        Shopping Cart ({items.length} {items.length === 1 ? "item" : "items"})
      </h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Cart Items */}
        <div className="lg:col-span-2 bg-white rounded-lg shadow-sm border border-gray-100 p-6">
          {items.map((item) => (
            <CartItem
              key={`${item.productId}-${item.variantSku || "default"}`}
              item={item}
            />
          ))}

          <div className="flex justify-between items-center mt-4 pt-4">
            <Link
              href="/products"
              className="inline-flex items-center space-x-1 text-blue-600 hover:underline text-sm"
            >
              <ArrowLeft size={16} />
              <span>Continue Shopping</span>
            </Link>
            <button
              onClick={clearCart}
              className="text-sm text-red-500 hover:text-red-700 transition-colors"
            >
              Clear Cart
            </button>
          </div>
        </div>

        {/* Order Summary */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-6 h-fit sticky top-24">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">
            Order Summary
          </h2>

          <div className="space-y-3 text-sm">
            <div className="flex justify-between">
              <span className="text-gray-600">Subtotal</span>
              <span className="font-medium">${subtotal.toFixed(2)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Tax (8%)</span>
              <span className="font-medium">${tax.toFixed(2)}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-gray-600">Shipping</span>
              <span className="font-medium">
                {shipping === 0 ? (
                  <span className="text-green-600">Free</span>
                ) : (
                  `$${shipping.toFixed(2)}`
                )}
              </span>
            </div>
            {subtotal < 100 && (
              <p className="text-xs text-gray-400">
                Add ${(100 - subtotal).toFixed(2)} more for free shipping!
              </p>
            )}
          </div>

          <div className="border-t border-gray-200 mt-4 pt-4">
            <div className="flex justify-between text-lg font-bold">
              <span>Total</span>
              <span>${total.toFixed(2)}</span>
            </div>
          </div>
          <Link
            href="/checkout"
            className="block w-full mt-6 bg-blue-600 text-white py-3 px-6 rounded-lg font-semibold hover:bg-blue-700 transition-colors text-center"
          >
            Proceed to Checkout
          </Link>
        </div>
      </div>
    </div>
  );
}
