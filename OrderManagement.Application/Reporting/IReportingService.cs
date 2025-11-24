namespace OrderManagement.Application.Reporting;

public interface IReportingService
{
    Task<object> GetSalesSummaryAsync(Guid tenantId, Guid? branchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<object> GetWaiterPerformanceAsync(Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<object> GetInventoryForecastAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<object> GetSubscriptionUsageAsync(Guid tenantId, CancellationToken cancellationToken);
}


