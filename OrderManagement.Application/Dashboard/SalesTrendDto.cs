namespace OrderManagement.Application.Dashboard;

public sealed record SalesTrendDto
{
    public DateTimeOffset Date { get; init; }
    public decimal Revenue { get; init; }
    public int OrderCount { get; init; }
}

