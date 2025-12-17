using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Payments;

public interface IPaymentService
{
    Task<PaymentTransaction> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        string provider,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken);

    Task<PaymentTransaction> RefundPaymentAsync(
        Guid paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken);

    Task<PaymentTransaction> VoidPaymentAsync(
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken);

    Task<PaymentTransaction?> GetPaymentByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);
}

