export enum OrderStatus {
  Submitted = 'Submitted',
  InPreparation = 'InPreparation',
  Ready = 'Ready',
  Served = 'Served',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
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
  status: OrderStatus;
  isTakeAway: boolean;
  tableNumber?: string;
  totalAmount: number;
  lines: OrderLine[];
  createdAt: string;
  updatedAt?: string;
}

