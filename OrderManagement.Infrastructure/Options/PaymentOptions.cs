namespace OrderManagement.Infrastructure.Options;

public sealed class PaymentOptions
{
    public string DefaultProvider { get; set; } = "Stripe";
    public StripeOptions Stripe { get; set; } = new();
    public IPay88Options IPay88 { get; set; } = new();
    public FpxOptions Fpx { get; set; } = new();

    public sealed class StripeOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }

    public sealed class IPay88Options
    {
        public string MerchantCode { get; set; } = string.Empty;
        public string MerchantKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }

    public sealed class FpxOptions
    {
        public string PartnerId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
    }
}


