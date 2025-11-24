namespace OrderManagement.Domain.Common;

public abstract class BranchScopedEntity : TenantScopedEntity
{
    public Guid BranchId { get; protected set; }

    protected BranchScopedEntity()
    {
    }

    protected BranchScopedEntity(Guid tenantId, Guid branchId) : base(tenantId)
    {
        BranchId = branchId;
    }
}


