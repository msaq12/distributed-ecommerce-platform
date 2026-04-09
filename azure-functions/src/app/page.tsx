"use client";

import Link from "next/link";
import { getAllProducts } from "@/lib/api";
import ProductGrid from "@/components/ProductGrid";
import { useState, useEffect } from "react";
import { Product } from "@/lib/types";

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getAllProducts().then((data) => {
      setProducts(data);
      setLoading(false);
    });
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading products...</p>
        </div>
      </div>
    );
  }

  const featuredProducts = products.filter((p) => p.isFeatured).slice(0, 4);
  const recentProducts = products.slice(0, 8);

  return (
    <div>
      {/* Hero Section */}
      <section className="bg-gradient-to-r from-blue-600 to-blue-800 text-white">
        <div className="max-w-7xl mx-auto px-4 py-20 sm:px-6 lg:px-8">
          <div className="max-w-2xl">
            <h1 className="text-4xl sm:text-5xl font-bold mb-6">
              Premium Furniture for Modern Living
            </h1>
            <p className="text-xl text-blue-100 mb-8">
              Discover our curated collection of high-quality furniture.
              From sofas to office chairs, find the perfect piece for every room.
            </p>
            <Link
              href="/products"
              className="inline-block bg-white text-blue-600 px-8 py-3 rounded-lg font-semibold hover:bg-gray-100 transition-colors"
            >
              Shop Now
            </Link>
          </div>
        </div>
      </section>

      {/* Featured Products */}
      {featuredProducts.length > 0 && (
        <section className="max-w-7xl mx-auto px-4 py-16 sm:px-6 lg:px-8">
          <ProductGrid products={featuredProducts} title="Featured Products" />
        </section>
      )}

      {/* All Products Preview */}
      <section className="max-w-7xl mx-auto px-4 py-16 sm:px-6 lg:px-8">
        <ProductGrid products={recentProducts} title="Our Collection" />
        {products.length > 8 && (
          <div className="text-center mt-8">
            <Link
              href="/products"
              className="inline-block bg-blue-600 text-white px-8 py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
            >
              View All Products
            </Link>
          </div>
        )}
      </section>

      {/* Categories Section */}
      <section className="bg-white py-16">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-2xl font-bold text-gray-900 mb-8 text-center">
            Shop by Category
          </h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            {[
              { name: "Sofas", emoji: "🛋️", id: "sofas" },
              { name: "Chairs", emoji: "🪑", id: "chairs" },
              { name: "Tables", emoji: "🪵", id: "tables" },
              { name: "Bedroom", emoji: "🛏️", id: "bedroom" },
            ].map((cat) => (
              <Link
                key={cat.id}
                href={`/products?category=${cat.id}`}
                className="flex flex-col items-center p-6 bg-gray-50 rounded-lg hover:bg-blue-50 hover:border-blue-200 border border-gray-200 transition-colors"
              >
                <span className="text-4xl mb-3">{cat.emoji}</span>
                <span className="font-medium text-gray-900">{cat.name}</span>
              </Link>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}
