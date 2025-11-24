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
}

