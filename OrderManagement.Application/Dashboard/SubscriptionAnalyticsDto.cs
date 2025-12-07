namespace OrderManagement.Application.Dashboard;

public sealed record SubscriptionStatusCountDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed record SubscriptionTrendDto
{
    public string Month { get; init; } = string.Empty;
    public int Count { get; init; }
}

