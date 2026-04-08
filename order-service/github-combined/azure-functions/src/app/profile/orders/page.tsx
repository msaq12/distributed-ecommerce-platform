"use client";

import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import ProtectedRoute from "@/components/protectedroute";
import Link from "next/link";
import { Order } from "@/lib/types";
import { getOrdersByCustomer } from "@/lib/api";

export default function OrderHistoryPage() {
  const { instance, accounts } = useMsal();
  const user = accounts[0];

  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchOrders() {
      try {
        const customerEmail =
          user?.username || user?.idTokenClaims?.emails?.[0];
        if (!customerEmail) {
          throw new Error("User email not found");
        }

        const tokenResponse = await instance.acquireTokenSilent({
          scopes: ["openid", "profile", "email"],
          account: accounts[0],
        });

        const data = await getOrdersByCustomer(
          customerEmail,
          tokenResponse.accessToken,
        );
        setOrders(data);
      } catch (err: any) {
        setError(err.message || "Failed to load orders");
      } finally {
        setLoading(false);
      }
    }

    if (user) {
      fetchOrders();
    }
  }, [user, instance, accounts]);

  if (loading) {
    return (
      <ProtectedRoute>
        <div className="flex items-center justify-center min-h-screen">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">Loading orders...</p>
          </div>
        </div>
      </ProtectedRoute>
    );
  }

  return (
    <ProtectedRoute>
      <div className="max-w-5xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-8">Order History</h1>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        {orders.length === 0 ? (
          <div className="bg-white rounded-lg shadow-md p-8 text-center">
            <p className="text-gray-600 mb-4">
              You haven't placed any orders yet.
            </p>
            <Link
              href="/products"
              className="inline-block bg-blue-600 text-white px-6 py-3 rounded-lg font-semibold hover:bg-blue-700"
            >
              Start Shopping
            </Link>
          </div>
        ) : (
          <div className="space-y-6">
            {orders.map((order) => (
              <div
                key={order.orderId}
                className="bg-white rounded-lg shadow-md p-6"
              >
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="font-bold text-lg">
                      Order #{order.orderId}
                    </h3>
                    <p className="text-sm text-gray-600">
                      Placed on {new Date(order.orderDate).toLocaleDateString()}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="font-bold text-lg">
                      ${order.totalAmount.toFixed(2)}
                    </p>
                    <span
                      className={`inline-block px-3 py-1 rounded-full text-sm font-medium ${
                        order.status === "Pending"
                          ? "bg-yellow-100 text-yellow-800"
                          : order.status === "Processing"
                            ? "bg-blue-100 text-blue-800"
                            : order.status === "Shipped"
                              ? "bg-green-100 text-green-800"
                              : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {order.status}
                    </span>
                  </div>
                </div>

                <div className="border-t pt-4">
                  <p className="text-sm text-gray-600 mb-2">Items:</p>
                  <ul className="space-y-1">
                    {order.items.map((item) => (
                      <li key={item.orderItemId} className="text-sm">
                        {item.productName} (x{item.quantity}) - $
                        {(item.unitPrice * item.quantity).toFixed(2)}
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="mt-4 flex gap-4">
                  <Link
                    href={`/order-confirmation/${order.orderId}`}
                    className="text-blue-600 hover:underline text-sm font-medium"
                  >
                    View Details
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </ProtectedRoute>
  );
}
