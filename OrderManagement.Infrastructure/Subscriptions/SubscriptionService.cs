using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions.Billing;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Subscriptions;

public sealed class SubscriptionService(
    OrderManagementDbContext dbContext,
    IPlanCatalog planCatalog) : ISubscriptionService
{
    public async Task<Subscription> CreateAsync(Guid tenantId, SubscriptionPlanCode planCode, bool autoRenew, CancellationToken cancellationToken)
    {
        var plan = planCatalog.GetPlan(planCode);
        var subscription = new Subscription(tenantId, planCode, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1), autoRenew)
        {
            // default values already set
        };

        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task ActivateAsync(Guid subscriptionId, string provider, string externalId, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions.FirstAsync(x => x.Id == subscriptionId, cancellationToken);
        subscription.Activate(provider, externalId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestPlanChangeAsync(Guid tenantId, SubscriptionPlanCode newPlan, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions.FirstAsync(x => x.TenantId == tenantId, cancellationToken);
        subscription.Suspend();
        subscription = new Subscription(tenantId, newPlan, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1), subscription.AutoRenew);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BillingPayload?> BuildBillingPayloadAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (tenant is null || subscription is null)
        {
            return null;
        }

        var plan = planCatalog.GetPlan(subscription.PlanCode);
        return new BillingPayload(
            tenant.Id,
            subscription.Id,
            plan.IncludesWhatsAppBilling ? "WhatsApp" : "Email",
            "billing@" + tenant.Code + ".com",
            plan.DisplayName,
            plan.MonthlyPrice,
            tenant.DefaultCurrency,
            subscription.StartDate,
            subscription.EndDate,
            null);
    }
}


