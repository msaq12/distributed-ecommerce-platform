import { useMsal } from "@azure/msal-react";
import { Link } from "react-router-dom";
import { LayoutDashboard, Package, ShoppingCart, LogOut } from "lucide-react";

export default function Navbar() {
  const { instance, accounts } = useMsal();
  const userName = accounts[0]?.name || "Admin User";

  const handleLogout = () => {
    instance.logoutRedirect();
  };

  return (
    <nav className="bg-blue-600 text-white shadow-lg">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <div className="flex items-center space-x-2">
            <Package size={32} />
            <span className="text-xl font-bold">Furniture Admin</span>
          </div>

          {/* Navigation Links */}
          <div className="flex space-x-6">
            <Link
              to="/"
              className="flex items-center space-x-2 hover:text-blue-200 transition"
            >
              <LayoutDashboard size={20} />
              <span>Dashboard</span>
            </Link>
            <Link
              to="/products"
              className="flex items-center space-x-2 hover:text-blue-200 transition"
            >
              <Package size={20} />
              <span>Products</span>
            </Link>
            <Link
              to="/orders"
              className="flex items-center space-x-2 hover:text-blue-200 transition"
            >
              <ShoppingCart size={20} />
              <span>Orders</span>
            </Link>
          </div>

          {/* User Info */}
          <div className="flex items-center space-x-4">
            <span className="text-sm">{userName}</span>
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 bg-blue-700 hover:bg-blue-800 px-4 py-2 rounded-md transition"
            >
              <LogOut size={18} />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </div>
    </nav>
  );
}
