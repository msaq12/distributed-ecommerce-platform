"use client";

import { useState, useEffect, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import {
  getAllProducts,
  getProductsByCategory,
  searchProducts,
} from "@/lib/api";
import ProductGrid from "@/components/ProductGrid";
import Link from "next/link";
import { Product } from "@/lib/types";

function ProductsContent() {
  const searchParams = useSearchParams();
  const query = searchParams.get("q");
  const category = searchParams.get("category");

  const [products, setProducts] = useState<Product[]>([]);
  const [allProducts, setAllProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageTitle, setPageTitle] = useState("All Products");

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);

      const all = await getAllProducts();
      setAllProducts(all);

      let filtered;
      let title = "All Products";

      if (query) {
        filtered = await searchProducts(query);
        title = `Search results for "${query}"`;
      } else if (category) {
        filtered = await getProductsByCategory(category);
        title = category.charAt(0).toUpperCase() + category.slice(1);
      } else {
        filtered = all;
      }

      setProducts(filtered);
      setPageTitle(title);
      setLoading(false);
    };

    fetchProducts();
  }, [query, category]);

  const categories = [...new Set(allProducts.map((p) => p.categoryId))].filter(
    Boolean,
  );

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

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
      <nav className="text-sm text-gray-500 mb-6">
        <Link href="/" className="hover:text-blue-600">
          Home
        </Link>
        <span className="mx-2">/</span>
        <span className="text-gray-900">Products</span>
        {category && (
          <>
            <span className="mx-2">/</span>
            <span className="text-gray-900 capitalize">{category}</span>
          </>
        )}
      </nav>

      <div className="flex flex-col md:flex-row gap-8">
        <aside className="w-full md:w-56 flex-shrink-0">
          <div className="bg-white rounded-lg shadow-sm p-4 border border-gray-100">
            <h3 className="font-semibold text-gray-900 mb-3">Categories</h3>
            <ul className="space-y-2">
              <li>
                <Link
                  href="/products"
                  className={`text-sm ${!category ? "text-blue-600 font-medium" : "text-gray-600 hover:text-blue-600"}`}
                >
                  All Products ({allProducts.length})
                </Link>
              </li>
              {categories.map((cat) => (
                <li key={cat}>
                  <Link
                    href={`/products?category=${cat}`}
                    className={`text-sm capitalize ${
                      category === cat
                        ? "text-blue-600 font-medium"
                        : "text-gray-600 hover:text-blue-600"
                    }`}
                  >
                    {cat} (
                    {allProducts.filter((p) => p.categoryId === cat).length})
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        </aside>

        <div className="flex-1">
          <div className="flex items-center justify-between mb-6">
            <h1 className="text-2xl font-bold text-gray-900">{pageTitle}</h1>
            <p className="text-sm text-gray-500">
              {products.length} product(s)
            </p>
          </div>
          <ProductGrid products={products} />
        </div>
      </div>
    </div>
  );
}

export default function ProductsPage() {
  return (
    <Suspense
      fallback={
        <div className="flex items-center justify-center min-h-screen">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
            <p className="text-gray-600">Loading...</p>
          </div>
        </div>
      }
    >
      <ProductsContent />
    </Suspense>
  );
}
