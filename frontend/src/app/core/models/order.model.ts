export enum OrderStatus {
  Draft = 1,
  Submitted = 2,
  InPreparation = 3,
  Ready = 4,
  Delivered = 5,
  Cancelled = 6,
  Paid = 7
}

export interface OrderLine {
  id?: string;
  menuItemId: string;
  name: string;
  price: number;
  quantity: number;
}

export interface CreateOrderRequest {
  isTakeAway: boolean;
  tableNumber?: string;
  items: OrderLine[];
}

export interface Order {
  id: string;
  tenantId: string;
  branchId: string;
  orderNumber: string;
  status: number | OrderStatus; // API returns integer, but we can use enum for type safety
  isTakeAway: boolean;
  tableNumber?: string;
  subtotal: number;
  tax: number;
  serviceCharge: number;
  total: number;
  lines: OrderLine[];
  createdAt: string;
  updatedAt?: string;
  cancellationReason?: string;
  paymentTiming?: number;
}

export interface CancelOrderRequest {
  reason: string;
}

export interface UpdateOrderLineRequest {
  quantity: number;
}

export interface Payment {
  id: string;
  orderId: string;
  provider: string;
  status: number;
  amount: number;
  currency: string;
  reference?: string;
  receiptUrl?: string;
}

export interface ProcessPaymentRequest {
  amount: number;
  provider: string;
  metadata?: Record<string, string>;
}

export interface RefundPaymentRequest {
  amount: number;
  reason: string;
}

export interface VoidPaymentRequest {
  reason: string;
}

export interface ReprintReceiptRequest {
  reason: string;
}

