using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class TenantUsageSnapshot : TenantScopedEntity
{
    public int ActiveBranches { get; private set; }
    public int ActiveUsers { get; private set; }
    public int OrdersCurrentMonth { get; private set; }
    public DateTimeOffset CapturedAt { get; private set; } = DateTimeOffset.UtcNow;

    private TenantUsageSnapshot()
    {
    }

    public TenantUsageSnapshot(Guid tenantId, int branches, int users, int orders) : base(tenantId)
    {
        ActiveBranches = branches;
        ActiveUsers = users;
        OrdersCurrentMonth = orders;
    }
}


