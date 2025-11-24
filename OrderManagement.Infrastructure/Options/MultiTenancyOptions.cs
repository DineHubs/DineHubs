namespace OrderManagement.Infrastructure.Options;

public sealed class MultiTenancyOptions
{
    public string Strategy { get; set; } = "Header";
    public string HeaderName { get; set; } = "X-Tenant-Code";
    public string BranchHeaderName { get; set; } = "X-Branch-Code";
    public Guid DefaultTenantId { get; set; }
}


