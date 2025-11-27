namespace OrderManagement.Application.Dashboard;

public sealed record TopSellingItemDto
{
    public Guid MenuItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int TotalQuantity { get; init; }
    public decimal TotalRevenue { get; init; }
}

