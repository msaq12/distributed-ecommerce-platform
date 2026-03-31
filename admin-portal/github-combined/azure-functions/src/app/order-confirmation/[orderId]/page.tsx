"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useMsal } from "@azure/msal-react";
import Link from "next/link";
import { CheckCircle } from "lucide-react";
import { getOrderById } from "@/lib/api";
import { Order } from "@/lib/types";

export default function OrderConfirmationPage() {
  const params = useParams();
  const orderId = params.orderId as string;
  const { instance, accounts } = useMsal();

  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchOrder() {
      try {
        const tokenResponse = await instance.acquireTokenSilent({
          scopes: ["openid", "profile", "email"],
          account: accounts[0],
        });

        const fetchedOrder = await getOrderById(
          orderId,
          tokenResponse.accessToken,
        );
        setOrder(fetchedOrder);
      } catch (err: any) {
        setError(err.message || "Failed to load order details");
      } finally {
        setLoading(false);
      }
    }

    if (orderId && accounts.length > 0) {
      fetchOrder();
    }
  }, [orderId, instance, accounts]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading order details...</p>
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="max-w-2xl mx-auto px-4 py-8">
        <div className="bg-red-50 border border-red-200 text-red-700 px-6 py-4 rounded-lg">
          <h2 className="text-xl font-bold mb-2">Error Loading Order</h2>
          <p>{error || "Order not found"}</p>
          <Link
            href="/"
            className="text-blue-600 hover:underline mt-4 inline-block"
          >
            Return to Home
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      {/* Success Header */}
      <div className="text-center mb-8">
        <CheckCircle size={64} className="text-green-500 mx-auto mb-4" />
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Order Confirmed!
        </h1>
        <p className="text-gray-600">
          Thank you for your order. We've sent a confirmation email to{" "}
          <span className="font-medium">{order.customerEmail}</span>
        </p>
      </div>

      {/* Order Details */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <div className="grid md:grid-cols-2 gap-4 mb-6">
          <div>
            <p className="text-sm text-gray-600">Order Number</p>
            <p className="font-medium text-lg">{order.orderId}</p>
          </div>
          <div>
            <p className="text-sm text-gray-600">Order Date</p>
            <p className="font-medium text-lg">
              {new Date(order.orderDate).toLocaleDateString()}
            </p>
          </div>
          <div>
            <p className="text-sm text-gray-600">Status</p>
            <p className="font-medium text-lg capitalize">{order.status}</p>
          </div>
          <div>
            <p className="text-sm text-gray-600">Total Amount</p>
            <p className="font-medium text-lg">
              ${order.totalAmount.toFixed(2)}
            </p>
          </div>
        </div>

        <div className="border-t pt-4">
          <p className="text-sm text-gray-600 mb-1">Shipping Address</p>
          <p className="font-medium">{order.shippingAddress}</p>
        </div>
      </div>

      {/* Order Items */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-6">
        <h2 className="text-xl font-bold mb-4">Order Items</h2>
        <div className="space-y-4">
          {order.items.map((item) => (
            <div
              key={item.orderItemId}
              className="flex justify-between items-center border-b pb-4 last:border-b-0"
            >
              <div>
                <p className="font-medium">{item.productName}</p>
                <p className="text-sm text-gray-600">
                  SKU: {item.sku}
                  {item.variantSku && ` • Variant: ${item.variantSku}`}
                </p>
                <p className="text-sm text-gray-600">
                  Quantity: {item.quantity}
                </p>
              </div>
              <p className="font-medium">
                ${(item.unitPrice * item.quantity).toFixed(2)}
              </p>
            </div>
          ))}
        </div>
      </div>

      {/* Next Steps */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 mb-6">
        <h3 className="font-bold text-lg mb-3">What's Next?</h3>
        <ul className="space-y-2 text-gray-700">
          <li>✅ Your order is being processed</li>
          <li>📧 You'll receive shipping updates via email</li>
          <li>📦 Estimated delivery: 5-7 business days</li>
        </ul>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-4">
        <Link
          href="/profile/orders"
          className="flex-1 bg-blue-600 text-white text-center py-3 rounded-lg font-semibold hover:bg-blue-700"
        >
          View Order History
        </Link>
        <Link
          href="/products"
          className="flex-1 bg-gray-200 text-gray-800 text-center py-3 rounded-lg font-semibold hover:bg-gray-300"
        >
          Continue Shopping
        </Link>
      </div>
    </div>
  );
}
