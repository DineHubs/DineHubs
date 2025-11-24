using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.Application.Payments;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Options;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Payments;

public sealed class StripePaymentGateway(
    OrderManagementDbContext dbContext,
    IOptions<PaymentOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    public string ProviderName => "Stripe";

    private readonly PaymentOptions.StripeOptions _stripe = options.Value.Stripe;

    public async Task<PaymentTransaction> CreatePaymentAsync(Guid orderId, decimal amount, string currency, Dictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FindAsync([orderId], cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        var payment = new PaymentTransaction(order.TenantId, order.BranchId, order.Id, ProviderName, amount, currency);
        payment.MarkAuthorized(Guid.NewGuid().ToString("N"));
        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created Stripe payment for order {Order}", orderId);
        return payment;
    }

    public Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received Stripe webhook with signature {Signature}", signature);
        // TODO: verify webhook using Stripe SDK
        return Task.FromResult(true);
    }
}


