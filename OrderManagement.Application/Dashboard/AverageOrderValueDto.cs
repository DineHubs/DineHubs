namespace OrderManagement.Application.Dashboard;

public sealed record AverageOrderValueDto
{
    public DateTimeOffset Date { get; init; }
    public decimal AverageOrderValue { get; init; }
    public int OrderCount { get; init; }
}

