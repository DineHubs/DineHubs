export interface DashboardStats {
  todayOrdersCount: number;
  todayRevenue: number;
  pendingOrdersCount: number;
  activeTablesCount: number;
  thisMonthRevenue: number;
  thisMonthOrdersCount: number;
  activeBranchesCount: number;
  activeUsersCount: number;
  ordersInQueueCount: number;
  inventoryValue: number;
  lowStockItemsCount: number;
  // Waiter-specific stats
  myOrdersTodayCount: number;
  myOrdersTodayRevenue: number;
  // SuperAdmin-specific stats
  activeTenantsCount: number;
  newSubscriptionsThisMonth: number;
  totalActiveSubscriptions: number;
}

export interface SalesTrend {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface TopSellingItem {
  menuItemId: string;
  name: string;
  totalQuantity: number;
  totalRevenue: number;
}

export interface OrderStatusCount {
  status: string;
  count: number;
}

export interface OrderHourlyCount {
  hour: number;
  orderCount: number;
}

export interface LowStockItem {
  itemId: string;
  name: string;
  quantityOnHand: number;
  reorderPoint: number;
  quantityNeeded: number;
}

export interface RevenueByDay {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface AverageOrderValue {
  date: string;
  averageOrderValue: number;
  orderCount: number;
}

export interface SubscriptionStatusCount {
  status: string;
  count: number;
}

export interface SubscriptionTrend {
  month: string;
  count: number;
}

