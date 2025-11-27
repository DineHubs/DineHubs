namespace OrderManagement.Application.Dashboard;

public sealed record LowStockItemDto
{
    public Guid ItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal QuantityOnHand { get; init; }
    public decimal ReorderPoint { get; init; }
    public decimal QuantityNeeded { get; init; }
}

