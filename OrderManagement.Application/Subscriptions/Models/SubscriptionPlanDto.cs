using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Subscriptions.Models;

public sealed record SubscriptionPlanDto(
    SubscriptionPlanCode Code,
    string DisplayName,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    int? DurationDays,
    int MaxBranches,
    int MaxUsers,
    bool IncludesInventory,
    bool IncludesAdvancedReporting,
    bool IncludesWhatsAppBilling);


