using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Payments;

public interface IPaymentGateway
{
    string ProviderName { get; }

    Task<PaymentTransaction> CreatePaymentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken);

    Task<bool> HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken);
}


