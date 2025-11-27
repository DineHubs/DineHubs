namespace OrderManagement.Application.Dashboard;

public sealed record OrderStatusCountDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
}

