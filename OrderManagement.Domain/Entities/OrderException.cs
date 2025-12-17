using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public enum ExceptionType
{
    ItemUnavailable = 1,
    PaymentFailure = 2,
    KitchenDelay = 3,
    CustomerRequest = 4
}

public class OrderException : BranchScopedEntity
{
    public Guid OrderId { get; private set; }
    public Guid? OrderLineId { get; private set; }
    public ExceptionType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? Resolution { get; private set; }
    public bool IsResolved { get; private set; }

    private OrderException()
    {
    }

    public OrderException(
        Guid tenantId,
        Guid branchId,
        Guid orderId,
        ExceptionType type,
        string description,
        Guid? orderLineId = null)
        : base(tenantId, branchId)
    {
        OrderId = orderId;
        OrderLineId = orderLineId;
        Type = type;
        Description = description;
        IsResolved = false;
    }

    public void Resolve(string resolution)
    {
        if (IsResolved)
        {
            throw new InvalidOperationException("Exception is already resolved.");
        }

        IsResolved = true;
        Resolution = resolution;
    }
}

