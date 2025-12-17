using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public enum PaymentTiming
{
    PayBeforeKitchen = 1,
    PayAfterReady = 2
}

public class Order : BranchScopedEntity
{
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    public decimal Subtotal { get; private set; }
    public decimal Tax { get; private set; }
    public decimal ServiceCharge { get; private set; }
    public decimal Total => Subtotal + Tax + ServiceCharge;
    public bool IsTakeAway { get; private set; }
    public string TableNumber { get; private set; } = string.Empty;
    public PaymentTiming PaymentTiming { get; private set; } = PaymentTiming.PayAfterReady;
    public string? CancellationReason { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines;

    private Order()
    {
    }

    public Order(Guid tenantId, Guid branchId, string orderNumber, bool isTakeAway, string tableNumber, PaymentTiming? paymentTiming = null)
        : base(tenantId, branchId)
    {
        OrderNumber = orderNumber;
        IsTakeAway = isTakeAway;
        TableNumber = tableNumber;
        PaymentTiming = paymentTiming ?? (isTakeAway ? PaymentTiming.PayBeforeKitchen : PaymentTiming.PayAfterReady);
    }

    public void AddLine(Guid menuItemId, string name, decimal price, int quantity)
    {
        _lines.Add(new OrderLine(menuItemId, name, price, quantity));
        RecalculateTotals();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status >= OrderStatus.InPreparation)
        {
            throw new InvalidOperationException("Cannot modify order lines after kitchen preparation has started.");
        }

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
        {
            throw new InvalidOperationException("Order line not found.");
        }

        _lines.Remove(line);
        RecalculateTotals();
    }

    public void UpdateLineQuantity(Guid lineId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be greater than zero.");
        }

        if (Status >= OrderStatus.InPreparation)
        {
            throw new InvalidOperationException("Cannot modify order lines after kitchen preparation has started.");
        }

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null)
        {
            throw new InvalidOperationException("Order line not found.");
        }

        // Create new line with updated quantity
        _lines.Remove(line);
        _lines.Add(new OrderLine(line.MenuItemId, line.Name, line.Price, quantity));
        RecalculateTotals();
    }

    public void Cancel(string reason)
    {
        if (Status >= OrderStatus.InPreparation)
        {
            throw new InvalidOperationException("Cannot cancel order after kitchen preparation has started.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        if (Status == OrderStatus.Paid)
        {
            throw new InvalidOperationException("Cannot cancel paid order. Process refund instead.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Cancellation reason is required.");
        }

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
    }

    public void UpdateStatus(OrderStatus status) => Status = status;

    public bool CanCancel()
    {
        return Status < OrderStatus.InPreparation 
            && Status != OrderStatus.Cancelled 
            && Status != OrderStatus.Paid;
    }

    public bool CanModifyLines()
    {
        return Status < OrderStatus.InPreparation;
    }

    private void RecalculateTotals()
    {
        Subtotal = _lines.Sum(l => l.Total);
        Tax = Math.Round(Subtotal * 0.06m, 2); // SST
        ServiceCharge = Math.Round(Subtotal * 0.1m, 2);
    }
}

public class OrderLine
{
    public Guid Id { get; private set; }
    public Guid MenuItemId { get; }
    public string Name { get; } = string.Empty;
    public decimal Price { get; }
    public int Quantity { get; }
    public decimal Total => Price * Quantity;

    private OrderLine()
    {
        Id = Guid.NewGuid();
    }

    public OrderLine(Guid menuItemId, string name, decimal price, int quantity)
    {
        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }
}

