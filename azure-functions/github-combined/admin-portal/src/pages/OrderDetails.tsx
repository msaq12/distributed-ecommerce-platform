import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { useParams, useNavigate } from "react-router-dom";
import { getOrderById, updateOrderStatus } from "../lib/api";
import type { Order, OrderStatus } from "../lib/types";
import { ArrowLeft, Package } from "lucide-react";
import LoadingSpinner from "../components/LoadingSpinner";
import { loginRequest } from "../lib/authConfig";

export default function OrderDetails() {
  const { instance, accounts } = useMsal();
  const { id } = useParams();
  const navigate = useNavigate();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadOrder();
  }, [id]);

  async function loadOrder() {
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      const data = await getOrderById(parseInt(id!), accessToken);
      setOrder(data);
    } catch (error) {
      console.error("Failed to load order:", error);
      alert("Failed to load order.");
      navigate("/orders");
    } finally {
      setLoading(false);
    }
  }

  async function handleStatusChange(newStatus: OrderStatus) {
    if (!order) return;
    console.log("Sending status:", newStatus, typeof newStatus);
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      await updateOrderStatus(order.orderId, newStatus, accessToken);
      alert("Order status updated!");
      loadOrder();
    } catch (error) {
      console.error("Failed to update order status:", error);
      alert("Failed to update order status.");
    }
  }

  if (loading) {
    return <LoadingSpinner />;
  }

  if (!order) {
    return (
      <div className="text-center py-12 text-gray-500">Order not found.</div>
    );
  }

  return (
    <div>
      <div className="flex items-center mb-6">
        <button
          onClick={() => navigate("/orders")}
          className="mr-4 text-gray-600 hover:text-gray-900"
        >
          <ArrowLeft size={24} />
        </button>
        <h1 className="text-3xl font-bold text-gray-800">
          Order #{order.orderNumber}
        </h1>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white p-6 rounded-lg shadow-md">
          <h2 className="text-xl font-bold text-gray-800 mb-4">Order Items</h2>

          <div className="space-y-4">
            {order.items.map((item) => (
              <div
                key={item.orderItemId}
                className="flex items-center border-b pb-4"
              >
                <div className="bg-gray-100 p-3 rounded">
                  <Package size={32} />
                </div>
                <div className="ml-4 flex-1">
                  <div className="font-semibold text-gray-900">
                    {item.productName}
                  </div>
                  <div className="text-sm text-gray-500">SKU: {item.sku}</div>
                  {item.variantSku && (
                    <div className="text-sm text-gray-500">
                      Variant: {item.variantSku}
                    </div>
                  )}
                  <div className="text-sm text-gray-500">
                    Quantity: {item.quantity}
                  </div>
                </div>
                <div className="text-right">
                  <div className="font-semibold text-gray-900">
                    ${(item.totalPrice || 0).toFixed(2)}
                  </div>
                  <div className="text-sm text-gray-500">
                    ${(item.unitPrice || 0).toFixed(2)} each
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="mt-6 border-t pt-4">
            <div className="flex justify-between text-lg font-bold text-gray-900">
              <span>Total</span>
              <span>${(order.totalAmount || 0).toFixed(2)}</span>
            </div>
          </div>
        </div>

        <div className="space-y-6">
          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-bold text-gray-800 mb-4">
              Customer Info
            </h3>
            <div className="space-y-2">
              <div>
                <span className="text-sm text-gray-600">Email:</span>
                <div className="font-medium text-gray-900">
                  {order.customerEmail}
                </div>
              </div>
              <div>
                <span className="text-sm text-gray-600">Shipping Address:</span>
                <div className="font-medium text-gray-900">
                  {order.shippingAddress}
                </div>
              </div>
              <div>
                <span className="text-sm text-gray-600">Order Date:</span>
                <div className="font-medium text-gray-900">
                  {new Date(order.orderDate).toLocaleString()}
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white p-6 rounded-lg shadow-md">
            <h3 className="text-lg font-bold text-gray-800 mb-4">
              Order Status
            </h3>
            <div className="space-y-3">
              <div className="text-sm text-gray-600">Current Status:</div>
              <div className="font-semibold text-gray-900 text-lg">
                {order.status}
              </div>

              <div className="mt-4">
                <label className="text-sm text-gray-600 block mb-2">
                  Update Status:
                </label>
                <select
                  value={order.status}
                  onChange={(e) =>
                    handleStatusChange(e.target.value as OrderStatus)
                  }
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="Pending">Pending</option>
                  <option value="Processing">Processing</option>
                  <option value="Shipped">Shipped</option>
                  <option value="Delivered">Delivered</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
