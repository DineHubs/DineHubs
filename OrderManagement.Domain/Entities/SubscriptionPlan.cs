using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public SubscriptionPlanCode Code { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal MonthlyPrice { get; private set; }
    public decimal AnnualPrice { get; private set; }
    public int MaxBranches { get; private set; }
    public int MaxUsers { get; private set; }
    public bool IncludesInventory { get; private set; }
    public bool IncludesAdvancedReporting { get; private set; }
    public bool IncludesWhatsAppBilling { get; private set; }

    private SubscriptionPlan()
    {
    }

    public SubscriptionPlan(
        SubscriptionPlanCode code,
        string name,
        decimal monthlyPrice,
        decimal annualPrice,
        int maxBranches,
        int maxUsers,
        bool includesInventory,
        bool includesAdvancedReporting,
        bool includesWhatsAppBilling)
    {
        Code = code;
        Name = name;
        MonthlyPrice = monthlyPrice;
        AnnualPrice = annualPrice;
        MaxBranches = maxBranches;
        MaxUsers = maxUsers;
        IncludesInventory = includesInventory;
        IncludesAdvancedReporting = includesAdvancedReporting;
        IncludesWhatsAppBilling = includesWhatsAppBilling;
    }
}


