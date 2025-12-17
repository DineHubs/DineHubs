using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class OrderMetrics : BranchScopedEntity
{
    public Guid OrderId { get; private set; }
    public TimeSpan? PrepTime { get; private set; }
    public TimeSpan? TableTurnTime { get; private set; }
    public bool? OrderAccuracy { get; private set; }

    private OrderMetrics()
    {
    }

    public OrderMetrics(
        Guid tenantId,
        Guid branchId,
        Guid orderId)
        : base(tenantId, branchId)
    {
        OrderId = orderId;
    }

    public void SetPrepTime(TimeSpan prepTime)
    {
        PrepTime = prepTime;
    }

    public void SetTableTurnTime(TimeSpan tableTurnTime)
    {
        TableTurnTime = tableTurnTime;
    }

    public void SetOrderAccuracy(bool isAccurate)
    {
        OrderAccuracy = isAccurate;
    }
}

