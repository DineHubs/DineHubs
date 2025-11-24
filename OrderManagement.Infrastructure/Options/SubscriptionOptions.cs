using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Options;

public sealed class SubscriptionOptions
{
    public List<SubscriptionPlanOption> Plans { get; set; } = [];
    public UsageThresholdOption UsageThresholds { get; set; } = new();

    public sealed class SubscriptionPlanOption
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public decimal AnnualPrice { get; set; }
        public int MaxBranches { get; set; }
        public int MaxUsers { get; set; }
        public bool IncludesInventory { get; set; }
        public bool IncludesWhatsAppBilling { get; set; }
        public bool IncludesAdvancedReporting { get; set; }
    }

    public sealed class UsageThresholdOption
    {
        public double Branches { get; set; } = 0.9;
        public double Users { get; set; } = 0.9;
        public double OrdersPerMonth { get; set; } = 0.9;
    }
}


