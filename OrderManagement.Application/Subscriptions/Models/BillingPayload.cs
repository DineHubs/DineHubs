namespace OrderManagement.Application.Subscriptions.Models;

public sealed record BillingPayload(
    Guid TenantId,
    Guid SubscriptionId,
    string Channel,
    string Recipient,
    string PlanName,
    decimal Amount,
    string Currency,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    string? InvoiceUrl);


