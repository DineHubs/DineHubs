namespace OrderManagement.Domain.Common;

public abstract class TenantScopedEntity : AuditableEntity
{
    public Guid TenantId { get; protected set; }

    protected TenantScopedEntity()
    {
    }

    protected TenantScopedEntity(Guid tenantId)
    {
        TenantId = tenantId;
    }
}


