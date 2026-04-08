import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { getAllProducts, getAllOrders } from "../lib/api";
import { Package, ShoppingCart, DollarSign, TrendingUp } from "lucide-react";
import LoadingSpinner from "../components/LoadingSpinner";
import { loginRequest } from "../lib/authConfig";

export default function Dashboard() {
  const { instance, accounts } = useMsal();
  const [stats, setStats] = useState({
    totalProducts: 0,
    totalOrders: 0,
    totalRevenue: 0,
    pendingOrders: 0,
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboardData();
  }, []);

  async function loadDashboardData() {
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      const [products, orders] = await Promise.all([
        getAllProducts(accessToken),
        getAllOrders(accessToken),
      ]);

      const totalRevenue = orders.reduce(
        (sum, order) => sum + order.totalAmount,
        0
      );
      const pendingOrders = orders.filter(
        (order) => order.status === "Pending"
      ).length;

      setStats({
        totalProducts: products.length,
        totalOrders: orders.length,
        totalRevenue,
        pendingOrders,
      });
    } catch (error) {
      console.error("Failed to load dashboard data:", error);
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <h1 className="text-3xl font-bold text-gray-800 mb-6">Dashboard</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Products</p>
              <p className="text-3xl font-bold text-gray-800">
                {stats.totalProducts}
              </p>
            </div>
            <div className="bg-blue-100 p-3 rounded-full">
              <Package className="text-blue-600" size={32} />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Orders</p>
              <p className="text-3xl font-bold text-gray-800">
                {stats.totalOrders}
              </p>
            </div>
            <div className="bg-green-100 p-3 rounded-full">
              <ShoppingCart className="text-green-600" size={32} />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Total Revenue</p>
              <p className="text-3xl font-bold text-gray-800">
                ${stats.totalRevenue.toLocaleString()}
              </p>
            </div>
            <div className="bg-purple-100 p-3 rounded-full">
              <DollarSign className="text-purple-600" size={32} />
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow-md">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Pending Orders</p>
              <p className="text-3xl font-bold text-gray-800">
                {stats.pendingOrders}
              </p>
            </div>
            <div className="bg-orange-100 p-3 rounded-full">
              <TrendingUp className="text-orange-600" size={32} />
            </div>
          </div>
        </div>
      </div>

      <div className="mt-8 bg-white p-6 rounded-lg shadow-md">
        <h2 className="text-xl font-bold text-gray-800 mb-4">
          Quick Actions
        </h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <a
            href="/products/new"
            className="bg-blue-600 text-white text-center py-3 rounded-lg hover:bg-blue-700 transition"
          >
            + Add New Product
          </a>
          <a
            href="/products"
            className="bg-green-600 text-white text-center py-3 rounded-lg hover:bg-green-700 transition"
          >
            View All Products
          </a>
          <a
            href="/orders"
            className="bg-purple-600 text-white text-center py-3 rounded-lg hover:bg-purple-700 transition"
          >
            View All Orders
          </a>
        </div>
      </div>
    </div>
  );
}
