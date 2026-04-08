"use client";

import { useState, useEffect } from "react";
import { useParams, useSearchParams } from "next/navigation";
import { getAllProducts, getProductById } from "@/lib/api";
import Link from "next/link";
import AddToCartSection from "./AddToCartSection";
import { Product } from "@/lib/types";

export default function ProductDetailPage() {
  const params = useParams();
  const searchParams = useSearchParams();
  const id = params.id as string;
  const categoryId = searchParams.get("categoryId");

  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchProduct = async () => {
      setLoading(true);
      let prod = null;

      if (categoryId) {
        prod = await getProductById(id, categoryId);
      } else {
        const allProducts = await getAllProducts();
        prod = allProducts.find((p) => p.id === id) || null;
      }

      setProduct(prod);
      setLoading(false);
    };

    fetchProduct();
  }, [id, categoryId]);

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading product...</p>
        </div>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center">
        <h1 className="text-2xl font-bold text-gray-900 mb-4">
          Product Not Found
        </h1>
        <p className="text-gray-500 mb-8">
          The product you're looking for doesn't exist.
        </p>
        <Link href="/products" className="text-blue-600 hover:underline">
          Browse all products
        </Link>
      </div>
    );
  }

  const primaryImage =
    product.images?.find((img) => img.isPrimary) || product.images?.[0];

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 sm:px-6 lg:px-8">
      {/* Breadcrumb */}
      <nav className="text-sm text-gray-500 mb-6">
        <Link href="/" className="hover:text-blue-600">
          Home
        </Link>
        <span className="mx-2">/</span>
        <Link href="/products" className="hover:text-blue-600">
          Products
        </Link>
        <span className="mx-2">/</span>
        <Link
          href={`/products?category=${product.categoryId}`}
          className="hover:text-blue-600 capitalize"
        >
          {product.categoryName || product.categoryId}
        </Link>
        <span className="mx-2">/</span>
        <span className="text-gray-900">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
        {/* Image Gallery */}
        <div>
          <div className="aspect-square bg-gray-100 rounded-lg overflow-hidden mb-4">
            {primaryImage ? (
              <img
                src={primaryImage.url}
                alt={primaryImage.altText || product.name}
                className="w-full h-full object-cover"
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center text-gray-400">
                <span className="text-9xl">🪑</span>
              </div>
            )}
          </div>
          {/* Thumbnail Gallery */}
          {product.images && product.images.length > 1 && (
            <div className="flex space-x-3">
              {product.images.map((img, idx) => (
                <div
                  key={idx}
                  className="w-20 h-20 bg-gray-100 rounded-lg overflow-hidden border-2 border-gray-200 cursor-pointer hover:border-blue-500"
                >
                  <img
                    src={img.url}
                    alt={img.altText}
                    className="w-full h-full object-cover"
                  />
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Product Info */}
        <div>
          {product.isFeatured && (
            <span className="inline-block bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full font-medium mb-3">
              Featured
            </span>
          )}

          <h1 className="text-3xl font-bold text-gray-900 mb-2">
            {product.name}
          </h1>

          <p className="text-sm text-gray-500 mb-4">
            SKU: {product.sku} | Category:{" "}
            {product.categoryName || product.categoryId}
          </p>

          <p className="text-gray-600 mb-6 leading-relaxed">
            {product.description}
          </p>

          {/* Add to Cart Section (client component) */}
          <AddToCartSection product={product} />

          {/* Product Details */}
          <div className="mt-8 border-t border-gray-200 pt-6">
            <h3 className="font-semibold text-gray-900 mb-3">
              Product Details
            </h3>
            <dl className="space-y-2 text-sm">
              {product.vendor && (
                <div className="flex">
                  <dt className="w-32 text-gray-500">Brand</dt>
                  <dd className="text-gray-900">{product.vendor}</dd>
                </div>
              )}
              {product.dimensions && (
                <div className="flex">
                  <dt className="w-32 text-gray-500">Dimensions</dt>
                  <dd className="text-gray-900">
                    {product.dimensions.length} x {product.dimensions.width} x{" "}
                    {product.dimensions.height} {product.dimensions.unit}
                  </dd>
                </div>
              )}
              {product.weight && (
                <div className="flex">
                  <dt className="w-32 text-gray-500">Weight</dt>
                  <dd className="text-gray-900">
                    {product.weight.value} {product.weight.unit}
                  </dd>
                </div>
              )}
              {product.tags && product.tags.length > 0 && (
                <div className="flex">
                  <dt className="w-32 text-gray-500">Tags</dt>
                  <dd className="flex flex-wrap gap-1">
                    {product.tags.map((tag) => (
                      <span
                        key={tag}
                        className="bg-gray-100 text-gray-600 text-xs px-2 py-1 rounded"
                      >
                        {tag}
                      </span>
                    ))}
                  </dd>
                </div>
              )}
            </dl>
          </div>
        </div>
      </div>
    </div>
  );
}
