using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Subscriptions;

public interface ISubscriptionService
{
    Task<Subscription> CreateAsync(Guid tenantId, SubscriptionPlanCode planCode, bool autoRenew, CancellationToken cancellationToken);
    Task ActivateAsync(Guid subscriptionId, string provider, string externalId, CancellationToken cancellationToken);
    Task RequestPlanChangeAsync(Guid tenantId, SubscriptionPlanCode newPlan, CancellationToken cancellationToken);
    Task<BillingPayload?> BuildBillingPayloadAsync(Guid tenantId, CancellationToken cancellationToken);
}


