namespace OrderManagement.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(DateTimeOffset from, DateTimeOffset to, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SalesTrendDto>> GetSalesTrendAsync(DateTimeOffset from, DateTimeOffset to, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TopSellingItemDto>> GetTopSellingItemsAsync(int count, DateTimeOffset from, DateTimeOffset to, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderStatusCountDto>> GetOrdersByStatusAsync(DateTimeOffset from, DateTimeOffset to, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderHourlyCountDto>> GetOrdersByHourAsync(DateTimeOffset date, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LowStockItemDto>> GetLowStockItemsAsync(Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RevenueByDayDto>> GetRevenueByDayAsync(DateTimeOffset from, DateTimeOffset to, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AverageOrderValueDto>> GetAverageOrderValueTrendAsync(DateTimeOffset from, DateTimeOffset to, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SubscriptionStatusCountDto>> GetSubscriptionStatusBreakdownAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SubscriptionTrendDto>> GetSubscriptionTrendAsync(int months, CancellationToken cancellationToken);
}

