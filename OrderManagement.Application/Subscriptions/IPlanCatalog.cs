using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Subscriptions;

public interface IPlanCatalog
{
    IReadOnlyCollection<SubscriptionPlanDto> GetPlans();
    SubscriptionPlanDto GetPlan(SubscriptionPlanCode code);
}


