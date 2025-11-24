using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Subscriptions;

public interface IUsageTracker
{
    Task<TenantUsageSnapshot> CaptureAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<bool> IsNearingLimitAsync(Guid tenantId, CancellationToken cancellationToken);
}


