using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class Subscription : TenantScopedEntity
{
    public SubscriptionPlanCode PlanCode { get; private set; }
    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Pending;
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public bool AutoRenew { get; private set; }
    public string BillingProvider { get; private set; } = "Stripe";
    public string? ExternalSubscriptionId { get; private set; }

    private Subscription()
    {
    }

    public Subscription(Guid tenantId, SubscriptionPlanCode planCode, DateTimeOffset startDate, DateTimeOffset endDate, bool autoRenew)
        : base(tenantId)
    {
        PlanCode = planCode;
        StartDate = startDate;
        EndDate = endDate;
        AutoRenew = autoRenew;
    }

    public void Activate(string provider, string externalId)
    {
        BillingProvider = provider;
        ExternalSubscriptionId = externalId;
        Status = SubscriptionStatus.Active;
    }

    public void Cancel() => Status = SubscriptionStatus.Cancelled;

    public void Suspend() => Status = SubscriptionStatus.Suspended;

    public void Renew(DateTimeOffset newEndDate)
    {
        StartDate = DateTimeOffset.UtcNow;
        EndDate = newEndDate;
        Status = SubscriptionStatus.Active;
    }
}


