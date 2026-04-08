"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ShoppingCart, Search, Menu, X, User, LogOut } from "lucide-react";
import { useCartStore } from "@/store/cartStore";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "@/lib/authConfig";

export default function Navbar() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const router = useRouter();
  const itemCount = useCartStore((state) => state.getItemCount());

  // MSAL hooks
  const { instance, accounts } = useMsal();
  const isAuthenticated = accounts.length > 0;
  const user = accounts[0];

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/products?q=${encodeURIComponent(searchQuery)}`);
      setSearchQuery("");
    }
  };

  const handleLogin = () => {
    instance.loginRedirect(loginRequest);
  };

  const handleLogout = () => {
    instance.logoutRedirect();
  };

  return (
    <nav className="bg-white shadow-md sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          {/* Logo */}
          <Link href="/" className="text-2xl font-bold text-blue-600">
            FurnitureDrop
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden md:flex items-center space-x-8">
            <Link
              href="/products"
              className="text-gray-700 hover:text-blue-600"
            >
              Products
            </Link>
            <Link href="/about" className="text-gray-700 hover:text-blue-600">
              About
            </Link>
            <Link href="/contact" className="text-gray-700 hover:text-blue-600">
              Contact
            </Link>
          </div>

          {/* Search Bar */}
          <form onSubmit={handleSearch} className="hidden md:flex items-center">
            <input
              type="text"
              placeholder="Search products..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-l-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button
              type="submit"
              className="bg-blue-600 text-white px-4 py-2 rounded-r-lg hover:bg-blue-700"
            >
              <Search size={20} />
            </button>
          </form>

          {/* Auth & Cart */}
          <div className="flex items-center space-x-4">
            {isAuthenticated ? (
              <>
                <Link
                  href="/profile"
                  className="flex items-center text-gray-700 hover:text-blue-600"
                >
                  <User size={20} className="mr-1" />
                  <span className="hidden md:inline">
                    {user?.name || user?.username || "Profile"}
                  </span>
                </Link>
                <button
                  onClick={handleLogout}
                  className="flex items-center text-gray-700 hover:text-blue-600"
                >
                  <LogOut size={20} className="mr-1" />
                  <span className="hidden md:inline">Sign Out</span>
                </button>
              </>
            ) : (
              <button
                onClick={handleLogin}
                className="flex items-center text-gray-700 hover:text-blue-600"
              >
                <User size={20} className="mr-1" />
                <span className="hidden md:inline">Sign In</span>
              </button>
            )}

            <Link
              href="/cart"
              className="relative text-gray-700 hover:text-blue-600"
            >
              <ShoppingCart size={24} />
              {itemCount > 0 && (
                <span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                  {itemCount}
                </span>
              )}
            </Link>

            {/* Mobile Menu Toggle */}
            <button
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="md:hidden text-gray-700"
            >
              {isMenuOpen ? <X size={24} /> : <Menu size={24} />}
            </button>
          </div>
        </div>

        {/* Mobile Menu */}
        {isMenuOpen && (
          <div className="md:hidden py-4 space-y-2">
            <Link
              href="/products"
              className="block text-gray-700 hover:text-blue-600 py-2"
            >
              Products
            </Link>
            <Link
              href="/about"
              className="block text-gray-700 hover:text-blue-600 py-2"
            >
              About
            </Link>
            <Link
              href="/contact"
              className="block text-gray-700 hover:text-blue-600 py-2"
            >
              Contact
            </Link>
            <form onSubmit={handleSearch} className="flex items-center py-2">
              <input
                type="text"
                placeholder="Search..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-l-lg"
              />
              <button
                type="submit"
                className="bg-blue-600 text-white px-4 py-2 rounded-r-lg"
              >
                <Search size={20} />
              </button>
            </form>
          </div>
        )}
      </div>
    </nav>
  );
}
