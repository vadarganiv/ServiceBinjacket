export interface ProductCategory {
  id: string;
  slug: string;
  name: string;
}

export interface ProductImage {
  path: string;
  alt: string;
}

export interface ProductListItem {
  id: number;
  slug: string;
  name: string;
  shortDescription: string | null;
  price: number;
  currency: string;
  condition: ProductCondition;
  image: ProductImage | null;
  inStock: boolean;
}

export interface ProductDetail extends ProductListItem {
  description: string | null;
  category: ProductCategory;
  warrantyMonths: number | null;
  images: ProductImage[];
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export type ProductCondition = 'New' | 'Used' | 'Refurbished' | 'Unknown';

export type SortOption = 'newest' | 'price_asc' | 'price_desc' | 'name_asc';

export interface ServiceCategory {
  id: number;
  name: string;
  slug: string;
  sortOrder: number;
}

export interface ServiceListItem {
  id: number;
  slug: string;
  name: string;
  shortDescription: string;
  priceNote: string | null;
  category: ServiceCategory | null;
}

export interface ServiceDetail extends ServiceListItem {
  description: string;
}

export interface RepairRequestResponse {
  id: number;
  status: string;
  createdAt: string;
}

export type DeliveryMethod = 'StorePickup' | 'LocalCourier' | 'PostalShipping' | 'ManualAgreement';
export type PaymentMethod = 'CashOnDelivery' | 'CashInStore';

export interface CartItem {
  productId: number;
  slug: string;
  name: string;
  price: number;
  currency: string;
  image: { path: string; alt: string } | null;
  quantity: number;
}

export interface OrderCreatePayload {
  customer: {
    fullName: string;
    phone: string;
    whatsAppPhone?: string;
    city: string;
    address?: string;
  };
  deliveryMethod: DeliveryMethod;
  paymentMethod: PaymentMethod;
  items: { productId: number; quantity: number }[];
  customerComment?: string;
  consent: boolean;
}

export interface OrderResponse {
  id: number;
  status: string;
  createdAt: string;
}
