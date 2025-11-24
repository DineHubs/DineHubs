using OrderManagement.Application.Abstractions;

namespace OrderManagement.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string? TenantCode { get; private set; }
    public string? BranchCode { get; private set; }

    public void SetTenant(Guid tenantId, string? tenantCode)
    {
        TenantId = tenantId;
        TenantCode = tenantCode;
    }

    public void SetBranch(Guid? branchId, string? branchCode)
    {
        BranchId = branchId;
        BranchCode = branchCode;
    }
}


