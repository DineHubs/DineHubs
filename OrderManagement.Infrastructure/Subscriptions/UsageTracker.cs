using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Options;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Subscriptions;

public sealed class UsageTracker(
    OrderManagementDbContext dbContext,
    IOptions<SubscriptionOptions> options) : IUsageTracker
{
    private readonly SubscriptionOptions.UsageThresholdOption _thresholds = options.Value.UsageThresholds;

    public async Task<TenantUsageSnapshot> CaptureAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var branches = await dbContext.Branches.CountAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken);
        var users = await dbContext.Users.CountAsync(x => x.TenantId == tenantId && x.LockoutEnd == null, cancellationToken);
        var currentMonth = DateTimeOffset.UtcNow;
        var orders = await dbContext.Orders.CountAsync(
            x => x.TenantId == tenantId && x.CreatedAt.Month == currentMonth.Month && x.CreatedAt.Year == currentMonth.Year,
            cancellationToken);

        var snapshot = new TenantUsageSnapshot(tenantId, branches, users, orders);
        dbContext.UsageSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    public async Task<bool> IsNearingLimitAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions.FirstAsync(x => x.TenantId == tenantId, cancellationToken);
        var plan = await dbContext.Plans.FirstOrDefaultAsync(x => x.Code == subscription.PlanCode, cancellationToken);
        if (plan is null)
        {
            return false;
        }

        var snapshot = await CaptureAsync(tenantId, cancellationToken);
        var branchLimitReached = plan.MaxBranches > 0 && snapshot.ActiveBranches >= plan.MaxBranches * _thresholds.Branches;
        var userLimitReached = plan.MaxUsers > 0 && snapshot.ActiveUsers >= plan.MaxUsers * _thresholds.Users;
        return branchLimitReached || userLimitReached;
    }
}

