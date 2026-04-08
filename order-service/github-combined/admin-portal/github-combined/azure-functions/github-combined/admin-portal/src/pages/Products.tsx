import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { Link } from "react-router-dom";
import { getAllProducts, searchProducts, deleteProduct } from "../lib/api";
import type { Product } from "../lib/types";
import { Search, Plus, Edit, Trash2 } from "lucide-react";
import LoadingSpinner from "../components/LoadingSpinner";
import { loginRequest } from "../lib/authConfig";

export default function Products() {
  const { instance, accounts } = useMsal();
  const [products, setProducts] = useState<Product[]>([]);
  const [filteredProducts, setFilteredProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");

  useEffect(() => {
    loadProducts();
  }, []);

  async function loadProducts() {
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      const data = await getAllProducts(accessToken);
      setProducts(data);
      setFilteredProducts(data);
    } catch (error) {
      console.error("Failed to load products:", error);
    } finally {
      setLoading(false);
    }
  }

  async function handleSearch(query: string) {
    setSearchQuery(query);

    if (!query.trim()) {
      setFilteredProducts(products);
      return;
    }

    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      const results = await searchProducts(query, accessToken);
      setFilteredProducts(results);
    } catch (error) {
      console.error("Search failed:", error);
    }
  }

  async function handleDelete(product: Product) {
    if (
      !confirm(
        `Are you sure you want to delete "${product.name}"? This will soft-delete the product.`,
      )
    ) {
      return;
    }

    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      await deleteProduct(product.id, product.categoryId, accessToken);
      alert("Product deleted successfully!");
      loadProducts();
    } catch (error) {
      console.error("Failed to delete product:", error);
      alert("Failed to delete product. See console for details.");
    }
  }

  if (loading) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Products</h1>
        <Link
          to="/products/new"
          className="flex items-center space-x-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition"
        >
          <Plus size={20} />
          <span>Add Product</span>
        </Link>
      </div>

      <div className="mb-6 relative">
        <Search
          className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"
          size={20}
        />
        <input
          type="text"
          placeholder="Search products by name or SKU..."
          value={searchQuery}
          onChange={(e) => handleSearch(e.target.value)}
          className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="bg-white rounded-lg shadow-md overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Product
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Category
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                SKU
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Price
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Stock
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {filteredProducts.map((product) => (
              <tr key={product.id} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center">
                    <img
                      src={product.images[0] || "/placeholder.png"}
                      alt={product.name}
                      className="h-10 w-10 rounded object-cover mr-3"
                    />
                    <div>
                      <div className="text-sm font-medium text-gray-900">
                        {product.name}
                      </div>
                      <div className="text-sm text-gray-500">
                        {product.isVariantProduct
                          ? `${product.variants?.length || 0} variants`
                          : "Simple product"}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {product.categoryName}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {product.sku}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {product.isVariantProduct && product.priceRange ? (
                    <span>
                      ${(product.priceRange.min || 0).toFixed(2)} - $
                      {(product.priceRange.max || 0).toFixed(2)}
                    </span>
                  ) : (
                    <span>${(product.basePrice || 0).toFixed(2)}</span>
                  )}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {product.stockQuantity}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span
                    className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                      product.isActive
                        ? "bg-green-100 text-green-800"
                        : "bg-red-100 text-red-800"
                    }`}
                  >
                    {product.isActive ? "Active" : "Inactive"}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <Link
                    to={`/products/edit/${product.id}?categoryId=${product.categoryId}`}
                    className="text-blue-600 hover:text-blue-900 mr-4"
                  >
                    <Edit size={18} className="inline" />
                  </Link>
                  <button
                    onClick={() => handleDelete(product)}
                    className="text-red-600 hover:text-red-900"
                  >
                    <Trash2 size={18} className="inline" />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {filteredProducts.length === 0 && (
          <div className="text-center py-12 text-gray-500">
            No products found.
          </div>
        )}
      </div>
    </div>
  );
}
