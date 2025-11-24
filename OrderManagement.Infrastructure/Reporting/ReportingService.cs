using System.Linq;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Reporting;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Reporting;

public sealed class ReportingService(OrderManagementDbContext dbContext) : IReportingService
{
    public async Task<object> GetSalesSummaryAsync(Guid tenantId, Guid? branchId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking().Where(o => o.TenantId == tenantId && o.CreatedAt >= from && o.CreatedAt <= to);
        if (branchId.HasValue)
        {
            query = query.Where(o => o.BranchId == branchId);
        }

        var total = await query.SumAsync(o => o.Total, cancellationToken);
        return new { Total = total, From = from, To = to };
    }

    public async Task<object> GetWaiterPerformanceAsync(Guid tenantId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var totalOrders = await dbContext.Orders
            .AsNoTracking()
            .CountAsync(o => o.TenantId == tenantId && o.CreatedAt >= from && o.CreatedAt <= to, cancellationToken);

        return new { TotalOrders = totalOrders };
    }

    public Task<object> GetInventoryForecastAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var items = dbContext.InventoryItems.AsNoTracking().Where(i => i.TenantId == tenantId)
            .Select(i => new { i.Name, i.QuantityOnHand, i.ReorderPoint })
            .ToArray();

        return Task.FromResult<object>(items);
    }

    public async Task<object> GetSubscriptionUsageAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.UsageSnapshots.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CapturedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (snapshot is null)
        {
            return new { ActiveBranches = 0, ActiveUsers = 0, OrdersCurrentMonth = 0 };
        }

        return new
        {
            snapshot.ActiveBranches,
            snapshot.ActiveUsers,
            snapshot.OrdersCurrentMonth,
            snapshot.CapturedAt
        };
    }
}

