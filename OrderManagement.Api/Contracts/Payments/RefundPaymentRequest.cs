namespace OrderManagement.Api.Contracts.Payments;

public sealed record RefundPaymentRequest(
    decimal Amount,
    string Reason);

