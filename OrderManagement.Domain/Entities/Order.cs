using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

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

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines;

    private Order()
    {
    }

    public Order(Guid tenantId, Guid branchId, string orderNumber, bool isTakeAway, string tableNumber)
        : base(tenantId, branchId)
    {
        OrderNumber = orderNumber;
        IsTakeAway = isTakeAway;
        TableNumber = tableNumber;
    }

    public void AddLine(Guid menuItemId, string name, decimal price, int quantity)
    {
        _lines.Add(new OrderLine(menuItemId, name, price, quantity));
        RecalculateTotals();
    }

    public void UpdateStatus(OrderStatus status) => Status = status;

    private void RecalculateTotals()
    {
        Subtotal = _lines.Sum(l => l.Total);
        Tax = Math.Round(Subtotal * 0.06m, 2); // SST
        ServiceCharge = Math.Round(Subtotal * 0.1m, 2);
    }
}

public class OrderLine
{
    public Guid MenuItemId { get; }
    public string Name { get; } = string.Empty;
    public decimal Price { get; }
    public int Quantity { get; }
    public decimal Total => Price * Quantity;

    private OrderLine()
    {
    }

    public OrderLine(Guid menuItemId, string name, decimal price, int quantity)
    {
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }
}

