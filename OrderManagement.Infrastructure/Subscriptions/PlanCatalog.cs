using Microsoft.Extensions.Options;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Options;

namespace OrderManagement.Infrastructure.Subscriptions;

public sealed class PlanCatalog(IOptions<SubscriptionOptions> options) : IPlanCatalog
{
    private readonly IReadOnlyCollection<SubscriptionPlanDto> _plans = options.Value.Plans
        .Select(Map)
        .ToArray();

    public IReadOnlyCollection<SubscriptionPlanDto> GetPlans() => _plans;

    public SubscriptionPlanDto GetPlan(SubscriptionPlanCode code) =>
        _plans.FirstOrDefault(p => p.Code == code)
        ?? throw new KeyNotFoundException($"Plan {code} not configured");

    private static SubscriptionPlanDto Map(SubscriptionOptions.SubscriptionPlanOption option)
    {
        var code = Enum.TryParse<SubscriptionPlanCode>(option.Name, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unknown plan code {option.Name}");

        return new SubscriptionPlanDto(
            code,
            option.DisplayName,
            option.MonthlyPrice,
            option.AnnualPrice,
            option.DurationDays,
            option.MaxBranches,
            option.MaxUsers,
            option.IncludesInventory,
            option.IncludesAdvancedReporting,
            option.IncludesWhatsAppBilling);
    }
}


