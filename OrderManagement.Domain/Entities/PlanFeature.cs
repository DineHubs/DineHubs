using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class PlanFeature : BaseEntity
{
    public Guid PlanId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool Enabled { get; private set; }

    private PlanFeature()
    {
    }

    public PlanFeature(Guid planId, string name, string description, bool enabled)
    {
        PlanId = planId;
        Name = name;
        Description = description;
        Enabled = enabled;
    }

    public void Toggle(bool enabled) => Enabled = enabled;
}


