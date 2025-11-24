using OrderManagement.Application.Subscriptions.Models;

namespace OrderManagement.Application.Abstractions.Billing;

public interface IBillingDispatcher
{
    Task SendInvoiceAsync(BillingPayload payload, CancellationToken cancellationToken = default);
}


