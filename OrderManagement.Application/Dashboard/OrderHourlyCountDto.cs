namespace OrderManagement.Application.Dashboard;

public sealed record OrderHourlyCountDto
{
    public int Hour { get; init; }
    public int OrderCount { get; init; }
}

