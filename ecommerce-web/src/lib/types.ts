// Product types matching Cosmos DB schema from Day 6

export interface ProductImage {
  url: string;
  altText: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface ProductVariant {
  variantId: string;
  sku: string;
  barcode?: string;
  name: string;
  attributes: VariantAttribute[];
  price: number;
  compareAtPrice?: number;
  costPrice?: number;
  image?: string;
  inventory: ProductInventory;
  isActive: boolean;
}

export interface VariantAttribute {
  name: string;
  value: string;
}

export interface ProductInventory {
  quantity: number;
  tracked: boolean;
  allowBackorder: boolean;
  lowStockThreshold?: number;
  warehouse?: string;
}

export interface PriceRange {
  min: number;
  max: number;
}

export interface ProductDimensions {
  length: number;
  width: number;
  height: number;
  unit: string;
}

export interface ProductWeight {
  value: number;
  unit: string;
}

export interface ProductSeo {
  title: string;
  description: string;
  handle: string;
}

export interface Product {
  id: string;
  categoryId: string;
  categoryName?: string;
  sku: string;
  barcode?: string;
  name: string;
  description: string;
  vendor?: string;
  supplier?: string;

  // Simple product fields
  price?: number;
  compareAtPrice?: number;
  costPrice?: number;
  inventory?: ProductInventory;

  // Variant product fields
  hasVariants: boolean;
  priceRange?: PriceRange;
  variants?: ProductVariant[];

  // Common fields
  images: ProductImage[];
  tags: string[];
  dimensions?: ProductDimensions;
  weight?: ProductWeight;
  seo?: ProductSeo;

  isActive: boolean;
  isDeleted: boolean;
  isFeatured: boolean;
  requiresShipping: boolean;
  taxable: boolean;

  createdAt: string;
  updatedAt: string;
}

// Cart types
export interface CartItem {
  productId: string;
  productName: string;
  sku: string;
  variantSku?: string;
  variantName?: string;
  price: number;
  quantity: number;
  image?: string;
  categoryId: string;
}

//Order types
// Order-related types
export interface CreateOrderRequest {
  customerEmail: string;
  shippingAddress: string;
  items: Array<{
    productId: string;
    productName: string;
    sku: string;
    variantSku: string | null;
    quantity: number;
    unitPrice: number;
  }>;
}

export interface Order {
  orderId: string;
  customerEmail: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  shippingAddress: string;
  items: Array<{
    orderItemId: string;
    productId: string;
    productName: string;
    sku: string;
    variantSku: string | null;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
  }>;
}
