using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class BillingHistory : TenantScopedEntity
{
    public string PlanCode { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "MYR";
    public DateTimeOffset PeriodStart { get; private set; }
    public DateTimeOffset PeriodEnd { get; private set; }
    public bool Paid { get; private set; }
    public string? InvoiceUrl { get; private set; }

    private BillingHistory()
    {
    }

    public BillingHistory(Guid tenantId, string planCode, decimal amount, DateTimeOffset periodStart, DateTimeOffset periodEnd)
        : base(tenantId)
    {
        PlanCode = planCode;
        Amount = amount;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public void MarkPaid(string? invoiceUrl)
    {
        Paid = true;
        InvoiceUrl = invoiceUrl;
    }
}


