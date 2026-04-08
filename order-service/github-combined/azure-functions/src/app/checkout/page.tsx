"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useMsal } from "@azure/msal-react";
import { useCartStore } from "@/store/cartStore";
import ProtectedRoute from "@/components/protectedroute";
import { createOrder } from "@/lib/api";
import { loginRequest } from "@/lib/authConfig";

interface CheckoutForm {
  shippingAddress: string;
  city: string;
  state: string;
  zipCode: string;
  cardNumber: string;
  cardExpiry: string;
  cardCvv: string;
}

export default function CheckoutPage() {
  const router = useRouter();
  const { instance, accounts } = useMsal();
  const user = accounts[0];
  const { items, getTotal, clearCart } = useCartStore();

  const [form, setForm] = useState<CheckoutForm>({
    shippingAddress: "",
    city: "",
    state: "",
    zipCode: "",
    cardNumber: "",
    cardExpiry: "",
    cardCvv: "",
  });

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const subtotal = getTotal();
  const tax = subtotal * 0.08;
  const shipping = subtotal > 100 ? 0 : 15;
  const total = subtotal + tax + shipping;

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    if (accounts.length === 0) {
      instance.loginRedirect(loginRequest);
      return;
    }

    try {
      const tokenResponse = await instance.acquireTokenSilent({
        scopes: ["openid", "profile", "email"],
        account: accounts[0],
      });

      const orderData = {
        customerEmail:
          user?.username ||
          user?.idTokenClaims?.emails?.[0] ||
          "unknown@email.com",
        customerName: user?.name || "Guest User",
        shippingAddress: `${form.shippingAddress}, ${form.city}, ${form.state} ${form.zipCode}`,
        items: items.map((item) => ({
          productId: item.productId,
          productName: item.productName,
          sku: item.sku,
          variantSku: item.variantSku || null,
          quantity: item.quantity,
          unitPrice: item.price,
        })),
      };

      const order = await createOrder(orderData, tokenResponse.accessToken);

      clearCart();

      router.push(`/order-confirmation/${order.orderId}`);
    } catch (err: any) {
      console.error("Order creation failed:", err);
      setError(err.message || "Failed to place order. Please try again.");
      setIsSubmitting(false);
    }
  };

  return (
    <ProtectedRoute>
      <div className="max-w-5xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-8">Checkout</h1>

        <div className="grid md:grid-cols-2 gap-8">
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h2 className="text-2xl font-bold mb-6">Shipping & Payment</h2>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Street Address
                </label>
                <input
                  type="text"
                  name="shippingAddress"
                  value={form.shippingAddress}
                  onChange={handleInputChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="123 Main St"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    City
                  </label>
                  <input
                    type="text"
                    name="city"
                    value={form.city}
                    onChange={handleInputChange}
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="New York"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    State
                  </label>
                  <input
                    type="text"
                    name="state"
                    value={form.state}
                    onChange={handleInputChange}
                    required
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    placeholder="NY"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  ZIP Code
                </label>
                <input
                  type="text"
                  name="zipCode"
                  value={form.zipCode}
                  onChange={handleInputChange}
                  required
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  placeholder="10001"
                />
              </div>

              <div className="mt-6">
                <h3 className="text-lg font-semibold mb-4">
                  Payment Information
                </h3>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Card Number
                  </label>
                  <input
                    type="text"
                    name="cardNumber"
                    value={form.cardNumber}
                    onChange={handleInputChange}
                    required
                    placeholder="1234 5678 9012 3456"
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4 mt-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Expiry Date
                    </label>
                    <input
                      type="text"
                      name="cardExpiry"
                      value={form.cardExpiry}
                      onChange={handleInputChange}
                      required
                      placeholder="MM/YY"
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      CVV
                    </label>
                    <input
                      type="text"
                      name="cardCvv"
                      value={form.cardCvv}
                      onChange={handleInputChange}
                      required
                      placeholder="123"
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                </div>
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
                  {error}
                </div>
              )}

              <button
                type="submit"
                disabled={isSubmitting || items.length === 0}
                className="w-full bg-blue-600 text-white py-3 rounded-lg font-semibold hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
              >
                {isSubmitting
                  ? "Processing..."
                  : `Place Order - $${total.toFixed(2)}`}
              </button>
            </form>
          </div>

          <div className="bg-white p-6 rounded-lg shadow-md h-fit">
            <h2 className="text-2xl font-bold mb-6">Order Summary</h2>

            <div className="space-y-4 mb-6">
              {items.map((item) => (
                <div
                  key={`${item.productId}-${item.variantSku}`}
                  className="flex justify-between"
                >
                  <div>
                    <p className="font-medium">{item.productName}</p>
                    <p className="text-sm text-gray-600">
                      {item.variantSku && `Variant: ${item.variantSku} • `}Qty:{" "}
                      {item.quantity}
                    </p>
                  </div>
                  <p className="font-medium">
                    ${(item.price * item.quantity).toFixed(2)}
                  </p>
                </div>
              ))}
            </div>

            <div className="border-t pt-4 space-y-2">
              <div className="flex justify-between text-gray-700">
                <span>Subtotal</span>
                <span>${subtotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-gray-700">
                <span>Tax (8%)</span>
                <span>${tax.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-gray-700">
                <span>Shipping</span>
                <span>
                  {shipping === 0 ? "FREE" : `$${shipping.toFixed(2)}`}
                </span>
              </div>
              <div className="flex justify-between text-xl font-bold pt-2 border-t">
                <span>Total</span>
                <span>${total.toFixed(2)}</span>
              </div>
            </div>

            {subtotal > 100 && (
              <div className="mt-4 bg-green-50 border border-green-200 text-green-700 px-4 py-2 rounded-lg text-sm">
                🎉 You qualify for FREE shipping!
              </div>
            )}
          </div>
        </div>
      </div>
    </ProtectedRoute>
  );
}
