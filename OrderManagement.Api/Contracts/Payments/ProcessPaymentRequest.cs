namespace OrderManagement.Api.Contracts.Payments;

public sealed record ProcessPaymentRequest(
    decimal Amount,
    string Provider,
    Dictionary<string, string>? Metadata = null);

