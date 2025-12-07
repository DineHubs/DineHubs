namespace OrderManagement.Application.Dashboard;

public sealed record DashboardStatsDto
{
    public int TodayOrdersCount { get; init; }
    public decimal TodayRevenue { get; init; }
    public int PendingOrdersCount { get; init; }
    public int ActiveTablesCount { get; init; }
    public decimal ThisMonthRevenue { get; init; }
    public int ThisMonthOrdersCount { get; init; }
    public int ActiveBranchesCount { get; init; }
    public int ActiveUsersCount { get; init; }
    public int OrdersInQueueCount { get; init; }
    public decimal InventoryValue { get; init; }
    public int LowStockItemsCount { get; init; }
    // Waiter-specific stats
    public int MyOrdersTodayCount { get; init; }
    public decimal MyOrdersTodayRevenue { get; init; }
    // SuperAdmin-specific stats
    public int ActiveTenantsCount { get; init; }
    public int NewSubscriptionsThisMonth { get; init; }
    public int TotalActiveSubscriptions { get; init; }
}

