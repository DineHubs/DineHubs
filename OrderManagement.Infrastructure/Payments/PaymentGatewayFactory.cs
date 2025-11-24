using OrderManagement.Application.Payments;

namespace OrderManagement.Infrastructure.Payments;

public sealed class PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways) : IPaymentGatewayFactory
{
    private readonly IReadOnlyDictionary<string, IPaymentGateway> _gateways = gateways.ToDictionary(
        g => g.ProviderName,
        StringComparer.OrdinalIgnoreCase);

    public IPaymentGateway GetGateway(string provider)
    {
        if (_gateways.TryGetValue(provider, out var gateway))
        {
            return gateway;
        }

        throw new InvalidOperationException($"Payment provider {provider} not registered");
    }
}


