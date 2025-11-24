using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class BillingMessageTemplate : TenantScopedEntity
{
    public string Channel { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    private BillingMessageTemplate()
    {
    }

    public BillingMessageTemplate(Guid tenantId, string channel, string subject, string body, bool isDefault)
        : base(tenantId)
    {
        Channel = channel;
        Subject = subject;
        Body = body;
        IsDefault = isDefault;
    }

    public void UpdateContent(string subject, string body)
    {
        Subject = subject;
        Body = body;
    }
}


