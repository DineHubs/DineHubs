using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Subscriptions.Models;

public sealed record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    SubscriptionPlanCode PlanCode,
    string PlanDisplayName,
    SubscriptionStatus Status,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    bool AutoRenew,
    string BillingProvider,
    string? ExternalSubscriptionId);

