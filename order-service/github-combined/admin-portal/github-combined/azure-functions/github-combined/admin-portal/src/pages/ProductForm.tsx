import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { createProduct, updateProduct, getProductById } from "../lib/api";
import type { CreateProductRequest, ProductVariant } from "../lib/types";
import { ArrowLeft, Plus, Trash2 } from "lucide-react";
import LoadingSpinner from "../components/LoadingSpinner";
import { loginRequest } from "../lib/authConfig";

export default function ProductForm() {
  const { instance, accounts } = useMsal();
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const categoryIdParam = searchParams.get("categoryId");
  const isEditMode = !!id;

  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState<CreateProductRequest>({
    name: "",
    description: "",
    categoryId: "",
    categoryName: "",
    basePrice: 0,
    sku: "",
    stockQuantity: 0,
    dimensions: { length: 0, width: 0, height: 0, unit: "cm" },
    weight: { value: 0, unit: "kg" },
    images: [],
    tags: [],
    isVariantProduct: false,
    variants: [],
  });

  useEffect(() => {
    if (isEditMode && id && categoryIdParam) {
      loadProduct();
    }
  }, [isEditMode, id, categoryIdParam]);

  async function loadProduct() {
    try {
      setLoading(true);
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      const product = await getProductById(id!, categoryIdParam!, accessToken);

      console.log("Loaded product from API:", JSON.stringify(product, null, 2));

      setFormData({
        name: product.name,
        description: product.description,
        categoryId: product.categoryId,
        categoryName: product.categoryName,
        basePrice: product.price || 0,
        sku: product.sku,
        stockQuantity: product.inventory?.quantity || 0,
        dimensions: product.dimensions,
        weight: product.weight || { value: 0, unit: "kg" },
        images: product.images,
        tags: product.tags,
        isVariantProduct: product.hasVariants || false,
        variants: product.variants || [],
      });
    } catch (error) {
      console.error("Failed to load product:", error);
      alert("Failed to load product.");
      navigate("/products");
    } finally {
      setLoading(false);
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    console.log("Submitting product data:", JSON.stringify(formData, null, 2));
    try {
      const response = await instance.acquireTokenSilent({
        ...loginRequest,
        account: accounts[0],
      });
      const accessToken = response.accessToken;

      if (isEditMode && id && categoryIdParam) {
        await updateProduct(id, categoryIdParam, formData, accessToken);
        alert("Product updated successfully!");
      } else {
        await createProduct(formData, accessToken);
        alert("Product created successfully!");
      }

      navigate("/products");
    } catch (error) {
      console.error("Failed to save product:", error);
      alert("Failed to save product. See console for details.");
    } finally {
      setLoading(false);
    }
  }

  function addVariant() {
    const newVariant: ProductVariant = {
      variantSku: "",
      variantName: "",
      attributes: {},
      price: 0,
      stockQuantity: 0,
      images: [],
    };
    setFormData({
      ...formData,
      variants: [...(formData.variants || []), newVariant],
    });
  }

  function removeVariant(index: number) {
    const newVariants = formData.variants?.filter((_, i) => i !== index);
    setFormData({ ...formData, variants: newVariants });
  }

  function updateVariant(index: number, field: string, value: any) {
    const newVariants = [...(formData.variants || [])];
    newVariants[index] = { ...newVariants[index], [field]: value };
    setFormData({ ...formData, variants: newVariants });
  }

  if (loading && isEditMode) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <div className="flex items-center mb-6">
        <button
          onClick={() => navigate("/products")}
          className="mr-4 text-gray-600 hover:text-gray-900"
        >
          <ArrowLeft size={24} />
        </button>
        <h1 className="text-3xl font-bold text-gray-800">
          {isEditMode ? "Edit Product" : "Create Product"}
        </h1>
      </div>

      <form
        onSubmit={handleSubmit}
        className="bg-white p-8 rounded-lg shadow-md"
      >
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Product Name *
            </label>
            <input
              type="text"
              required
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              SKU *
            </label>
            <input
              type="text"
              required
              value={formData.sku}
              onChange={(e) =>
                setFormData({ ...formData, sku: e.target.value })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Description *
          </label>
          <textarea
            required
            rows={4}
            value={formData.description}
            onChange={(e) =>
              setFormData({ ...formData, description: e.target.value })
            }
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Category ID *
            </label>
            <input
              type="text"
              required
              value={formData.categoryId}
              onChange={(e) =>
                setFormData({ ...formData, categoryId: e.target.value })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Category Name *
            </label>
            <input
              type="text"
              required
              value={formData.categoryName}
              onChange={(e) =>
                setFormData({ ...formData, categoryName: e.target.value })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Base Price ($) *
            </label>
            <input
              type="number"
              required
              step="0.01"
              value={formData.basePrice || 0}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  basePrice: parseFloat(e.target.value) || 0,
                })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Stock Quantity *
            </label>
            <input
              type="number"
              required
              value={formData.stockQuantity || 0}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  stockQuantity: parseInt(e.target.value) || 0,
                })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Length (cm) *
            </label>
            <input
              type="number"
              required
              value={formData?.dimensions?.length || 0}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  dimensions: {
                    ...formData.dimensions,
                    length: parseFloat(e.target.value) || 0,
                  },
                })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Width (cm) *
            </label>
            <input
              type="number"
              required
              value={formData?.dimensions?.width || 0}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  dimensions: {
                    ...formData.dimensions,
                    width: parseFloat(e.target.value) || 0,
                  },
                })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Height (cm) *
            </label>
            <input
              type="number"
              required
              value={formData?.dimensions?.height || 0}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  dimensions: {
                    ...formData.dimensions,
                    height: parseFloat(e.target.value) || 0,
                  },
                })
              }
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Weight (kg) *
          </label>
          <input
            type="number"
            required
            step="0.01"
            value={formData.weight.value || 0}
            onChange={(e) =>
              setFormData({
                ...formData,
                weight: {
                  ...formData.weight,
                  value: parseFloat(e.target.value) || 0,
                },
              })
            }
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Image URLs (comma-separated)
          </label>
          <input
            type="text"
            value={formData.images.join(", ")}
            onChange={(e) =>
              setFormData({
                ...formData,
                images: e.target.value
                  .split(",")
                  .map((url) => url.trim())
                  .filter(Boolean),
              })
            }
            placeholder="https://example.com/image1.jpg, https://example.com/image2.jpg"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Tags (comma-separated)
          </label>
          <input
            type="text"
            value={formData.tags.join(", ")}
            onChange={(e) =>
              setFormData({
                ...formData,
                tags: e.target.value
                  .split(",")
                  .map((tag) => tag.trim())
                  .filter(Boolean),
              })
            }
            placeholder="modern, luxury, bestseller"
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div className="mb-6">
          <label className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={formData.isVariantProduct}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  isVariantProduct: e.target.checked,
                  variants: e.target.checked ? formData.variants : [],
                })
              }
              className="w-5 h-5"
            />
            <span className="text-sm font-medium text-gray-700">
              This is a variant product (has color/size options)
            </span>
          </label>
        </div>

        {formData.isVariantProduct && (
          <div className="mb-6 border-t pt-6">
            <div className="flex justify-between items-center mb-4">
              <h3 className="text-lg font-bold text-gray-800">Variants</h3>
              <button
                type="button"
                onClick={addVariant}
                className="flex items-center space-x-2 bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition"
              >
                <Plus size={18} />
                <span>Add Variant</span>
              </button>
            </div>

            {formData.variants?.map((variant, index) => (
              <div
                key={index}
                className="border border-gray-300 rounded-lg p-4 mb-4"
              >
                <div className="flex justify-between items-center mb-4">
                  <h4 className="font-semibold text-gray-700">
                    Variant {index + 1}
                  </h4>
                  <button
                    type="button"
                    onClick={() => removeVariant(index)}
                    className="text-red-600 hover:text-red-800"
                  >
                    <Trash2 size={18} />
                  </button>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Variant SKU
                    </label>
                    <input
                      type="text"
                      value={variant.variantSku}
                      onChange={(e) =>
                        updateVariant(index, "variantSku", e.target.value)
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Variant Name
                    </label>
                    <input
                      type="text"
                      value={variant.variantName}
                      onChange={(e) =>
                        updateVariant(index, "variantName", e.target.value)
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Price ($)
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      value={variant.price || 0}
                      onChange={(e) =>
                        updateVariant(
                          index,
                          "price",
                          parseFloat(e.target.value) || 0,
                        )
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Stock Quantity
                    </label>
                    <input
                      type="number"
                      value={variant.stockQuantity || 0}
                      onChange={(e) =>
                        updateVariant(
                          index,
                          "stockQuantity",
                          parseInt(e.target.value) || 0,
                        )
                      }
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-2">
                      Attributes (JSON format)
                    </label>
                    <input
                      type="text"
                      value={JSON.stringify(variant.attributes)}
                      onChange={(e) => {
                        try {
                          const parsed = JSON.parse(e.target.value);
                          updateVariant(index, "attributes", parsed);
                        } catch {
                          // Invalid JSON, ignore
                        }
                      }}
                      placeholder='{"Color": "Blue", "Size": "Large"}'
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        <div className="flex justify-end space-x-4">
          <button
            type="button"
            onClick={() => navigate("/products")}
            className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:bg-gray-400"
          >
            {loading
              ? "Saving..."
              : isEditMode
                ? "Update Product"
                : "Create Product"}
          </button>
        </div>
      </form>
    </div>
  );
}
