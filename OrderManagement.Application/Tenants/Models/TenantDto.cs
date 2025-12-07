namespace OrderManagement.Application.Tenants.Models;

public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string CountryCode { get; init; } = string.Empty;
    public string DefaultCurrency { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    
    // Subscription information
    public string? SubscriptionStatus { get; init; }
    public string? SubscriptionPlanCode { get; init; }
    public DateTimeOffset? SubscriptionStartDate { get; init; }
    public DateTimeOffset? SubscriptionEndDate { get; init; }
    public bool? SubscriptionAutoRenew { get; init; }
}

public sealed record TenantDetailDto : TenantDto
{
    public int BranchesCount { get; init; }
    public int UsersCount { get; init; }
    public SubscriptionDetailDto? Subscription { get; init; }
}

public sealed record SubscriptionDetailDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PlanCode { get; init; } = string.Empty;
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public bool AutoRenew { get; init; }
    public string BillingProvider { get; init; } = string.Empty;
    public string? ExternalSubscriptionId { get; init; }
}

