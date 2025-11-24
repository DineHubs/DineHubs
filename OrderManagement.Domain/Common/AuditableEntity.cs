namespace OrderManagement.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public void SetCreatedBy(Guid userId) => CreatedBy ??= userId;

    public void Touch(Guid userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}


