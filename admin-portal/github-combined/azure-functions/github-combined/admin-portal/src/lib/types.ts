// ==================== PRODUCTS ====================

export interface ProductVariant {
  variantSku: string;
  variantName: string;
  attributes: {
    [key: string]: string; // e.g., { "Color": "Blue", "Size": "Large" }
  };
  price: number;
  stockQuantity: number;
  images: string[];
}
export interface Product {
  id: string;
  name: string;
  description: string;
  categoryId: string;
  categoryName: string;
  basePrice?: number;
  price?: number;
  priceRange?: {
    min: number;
    max: number;
  };
  sku: string;
  stockQuantity?: number;
  inventory?: {
    quantity: number;
  } | null;
  dimensions: {
    length: number;
    width: number;
    height: number;
    unit: string;
  };
  weight: {
    value: number;
    unit: string;
  };
  images: string[];
  tags: string[];
  isActive: boolean;
  isVariantProduct?: boolean;
  hasVariants?: boolean;
  variants?: ProductVariant[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  categoryId: string;
  categoryName: string;
  basePrice: number;
  sku: string;
  stockQuantity: number;
  dimensions: {
    length: number;
    width: number;
    height: number;
    unit: string;
  };
  weight: {
    value: number;
    unit: string;
  };
  images: string[];
  tags: string[];
  isVariantProduct: boolean;
  variants?: ProductVariant[];
}

// ==================== ORDERS ====================

export interface Order {
  orderId: number;
  orderNumber: string;
  customerEmail: string;
  orderDate: string;
  status: OrderStatus;
  totalAmount: number;
  shippingAddress: string;
  items: OrderItem[];
}

export interface OrderItem {
  orderItemId: number;
  productId: number;
  productName: string;
  sku: string;
  variantSku: string | null;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export type OrderStatus =
  | "Pending"
  | "Processing"
  | "Shipped"
  | "Delivered"
  | "Cancelled";

// ==================== API RESPONSES ====================

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
