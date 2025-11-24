namespace OrderManagement.Infrastructure.Options;

public sealed class WhatsAppOptions
{
    public string Provider { get; set; } = "MetaCloud";
    public string BaseUrl { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string DefaultPhoneNumberId { get; set; } = string.Empty;
}


