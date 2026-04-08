import type {
  Product,
  CreateProductRequest,
  Order,
  OrderStatus,
} from "./types";

//const APIM_URL = import.meta.env.VITE_APIM_URL;
const APIM_KEY = import.meta.env.VITE_APIM_KEY;
const PRODUCT_API_URL = import.meta.env.VITE_PRODUCT_API_URL;
const ORDER_API_URL = import.meta.env.VITE_ORDER_API_URL;

// Helper function to create headers with Bearer token
function getHeaders(accessToken: string): HeadersInit {
  return {
    "Content-Type": "application/json",
    "Ocp-Apim-Subscription-Key": APIM_KEY,
    Authorization: `Bearer ${accessToken}`,
  };
}

function statusToId(status: OrderStatus): number {
  const map: Record<OrderStatus, number> = {
    Pending: 1,
    Processing: 2,
    Shipped: 3,
    Delivered: 4,
    Cancelled: 5,
  };
  return map[status];
}

// ==================== PRODUCT API ====================

export async function getAllProducts(accessToken: string): Promise<Product[]> {
  const response = await fetch(`${PRODUCT_API_URL}/api/products`, {
    headers: getHeaders(accessToken),
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch products: ${response.statusText}`);
  }

  return response.json();
}

export async function getProductById(
  id: string,
  categoryId: string,
  accessToken: string,
): Promise<Product> {
  const response = await fetch(
    `${PRODUCT_API_URL}/api/products/${id}?categoryId=${categoryId}`,
    {
      headers: getHeaders(accessToken),
    },
  );

  if (!response.ok) {
    throw new Error(`Failed to fetch product: ${response.statusText}`);
  }

  return response.json();
}

export async function searchProducts(
  query: string,
  accessToken: string,
): Promise<Product[]> {
  const response = await fetch(
    `${PRODUCT_API_URL}/api/products/search?q=${encodeURIComponent(query)}`,
    {
      headers: getHeaders(accessToken),
    },
  );

  if (!response.ok) {
    throw new Error(`Failed to search products: ${response.statusText}`);
  }

  return response.json();
}

export async function createProduct(
  product: CreateProductRequest,
  accessToken: string,
): Promise<Product> {
  const payload = {
    ...product,
    price: product.basePrice,
  };

  const response = await fetch(`${PRODUCT_API_URL}/api/products`, {
    method: "POST",
    headers: getHeaders(accessToken),
    body: JSON.stringify(payload), // ← Use payload here, not product
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Failed to create product: ${errorText}`);
  }

  return response.json();
}

export async function updateProduct(
  id: string,
  categoryId: string,
  product: Partial<CreateProductRequest>,
  accessToken: string,
): Promise<Product> {
  const payload = {
    ...product,
    price: product.basePrice,
  };

  const response = await fetch(
    `${PRODUCT_API_URL}/api/products/${id}?categoryId=${categoryId}`,
    {
      method: "PUT",
      headers: getHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  );

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Failed to update product: ${errorText}`);
  }

  return response.json();
}

export async function deleteProduct(
  id: string,
  categoryId: string,
  accessToken: string,
): Promise<void> {
  const response = await fetch(
    `${PRODUCT_API_URL}/api/products/${id}?categoryId=${categoryId}`,
    {
      method: "DELETE",
      headers: getHeaders(accessToken),
    },
  );

  if (!response.ok) {
    throw new Error(`Failed to delete product: ${response.statusText}`);
  }
}

// ==================== ORDER API ====================

export async function getAllOrders(accessToken: string): Promise<Order[]> {
  const response = await fetch(`${ORDER_API_URL}/api/orders`, {
    headers: getHeaders(accessToken),
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch orders: ${response.statusText}`);
  }

  return response.json();
}

export async function getOrderById(
  orderId: number,
  accessToken: string,
): Promise<Order> {
  const response = await fetch(`${ORDER_API_URL}/api/orders/${orderId}`, {
    headers: getHeaders(accessToken),
  });

  if (!response.ok) {
    throw new Error(`Failed to fetch order: ${response.statusText}`);
  }

  return response.json();
}

export async function updateOrderStatus(
  orderId: number,
  status: OrderStatus,
  accessToken: string,
): Promise<Order> {
  const response = await fetch(
    `${ORDER_API_URL}/api/orders/${orderId}/status`,
    {
      method: "PUT",
      headers: getHeaders(accessToken),
      body: JSON.stringify({ orderStatusId: statusToId(status) }),
    },
  );

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Failed to update order status: ${errorText}`);
  }

  return response.json();
}
