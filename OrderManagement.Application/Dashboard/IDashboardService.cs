namespace OrderManagement.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SalesTrendDto>> GetSalesTrendAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TopSellingItemDto>> GetTopSellingItemsAsync(int count, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderStatusCountDto>> GetOrdersByStatusAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderHourlyCountDto>> GetOrdersByHourAsync(DateTimeOffset date, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LowStockItemDto>> GetLowStockItemsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RevenueByDayDto>> GetRevenueByDayAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AverageOrderValueDto>> GetAverageOrderValueTrendAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}

