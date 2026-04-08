const APIM_BASE_URL =
  process.env.NEXT_PUBLIC_APIM_URL ||
  "https://apim-furniture-dev.azure-api.net";
const APIM_KEY = process.env.NEXT_PUBLIC_APIM_KEY || "";
const ORDER_API_URL = process.env.NEXT_PUBLIC_ORDER_API_URL || "";

// For local development fallback (direct to Product Service)
const PRODUCT_API_URL =
  process.env.NEXT_PUBLIC_PRODUCT_API_URL || `${APIM_BASE_URL}/products`;

import { Product, CreateOrderRequest, Order } from "./types";
import { useMsal } from "@azure/msal-react";

async function apiFetch(url: string): Promise<Response> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };

  if (APIM_KEY) {
    headers["Ocp-Apim-Subscription-Key"] = APIM_KEY;
  }

  const response = await fetch(url, { headers });

  if (!response.ok) {
    throw new Error(`API error: ${response.status} ${response.statusText}`);
  }

  return response;
}

// Get all products
export async function getAllProducts(): Promise<Product[]> {
  try {
    const response = await apiFetch(`${PRODUCT_API_URL}/api/products`);
    return response.json();
  } catch (error) {
    console.error("Failed to fetch products:", error);
    return [];
  }
}

// Get products by category
export async function getProductsByCategory(
  categoryId: string,
): Promise<Product[]> {
  try {
    const response = await apiFetch(
      `${PRODUCT_API_URL}/api/products/category/${categoryId}`,
    );
    return response.json();
  } catch (error) {
    console.error(
      `Failed to fetch products for category ${categoryId}:`,
      error,
    );
    return [];
  }
}

// Get single product by ID
export async function getProductById(
  id: string,
  categoryId: string,
): Promise<Product | null> {
  try {
    const response = await apiFetch(
      `${PRODUCT_API_URL}/api/products/${id}?categoryId=${categoryId}`,
    );
    return response.json();
  } catch (error) {
    console.error(`Failed to fetch product ${id}:`, error);
    return null;
  }
}

// Search products
export async function searchProducts(query: string): Promise<Product[]> {
  try {
    const response = await apiFetch(
      `${PRODUCT_API_URL}/api/products/search?q=${encodeURIComponent(query)}`,
    );
    return response.json();
  } catch (error) {
    console.error(`Search failed for "${query}":`, error);
    return [];
  }
}

// Get product variants
export async function getProductVariants(
  id: string,
  categoryId: string,
): Promise<Product | null> {
  try {
    const response = await apiFetch(
      `${PRODUCT_API_URL}/api/products/${id}/variants?categoryId=${categoryId}`,
    );
    return response.json();
  } catch (error) {
    console.error(`Failed to fetch variants for ${id}:`, error);
    return null;
  }
}

/**
 * Create a new order
 */
export async function createOrder(
  orderData: CreateOrderRequest,
  accessToken: string,
): Promise<Order> {
  try {
    console.log("Token:", accessToken ? "Present" : "Missing");
    console.log("Token length:", accessToken?.length);
    console.log("Full token:", accessToken);
    console.log("Calling:", `${ORDER_API_URL}`);
    const apiResponse = await fetch(`${ORDER_API_URL}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Ocp-Apim-Subscription-Key": APIM_KEY,
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify(orderData),
    });

    if (!apiResponse.ok) {
      console.log("Response status:", apiResponse.status);
      const errorText = await apiResponse.text();
      console.log("Response body:", errorText);
      throw new Error(
        `Failed to create order: ${apiResponse.status} - ${errorText}`,
      );
    }

    const order: Order = await apiResponse.json();
    return order;
  } catch (error) {
    console.error("Error creating order:", error);
    throw error;
  }
}

/**
 * Get order by ID
 */
export async function getOrderById(
  orderId: string,
  accessToken: string,
): Promise<Order> {
  try {
    const response = await fetch(`${ORDER_API_URL}/${orderId}`, {
      headers: {
        "Ocp-Apim-Subscription-Key": APIM_KEY,
        Authorization: `Bearer ${accessToken}`,
      },
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch order: ${response.status}`);
    }

    const order: Order = await response.json();
    return order;
  } catch (error) {
    console.error("Error fetching order:", error);
    throw error;
  }
}

export async function getOrdersByCustomer(
  customerEmail: string,
  accessToken: string,
): Promise<Order[]> {
  try {
    const response = await fetch(
      `http://135.237.18.70/api/orders/customer/${encodeURIComponent(customerEmail)}`, // Full URL
      {
        headers: {
          "Ocp-Apim-Subscription-Key": APIM_KEY,
          Authorization: `Bearer ${accessToken}`,
        },
      },
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch orders: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching orders:", error);
    throw error;
  }
}
