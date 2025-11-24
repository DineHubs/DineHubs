using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class Branch : TenantScopedEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private Branch()
    {
    }

    public Branch(Guid tenantId, string name, string code, string address, string city) : base(tenantId)
    {
        Name = name;
        Code = code;
        Address = address;
        City = city;
    }

    public void UpdateDetails(string address, string city)
    {
        Address = address;
        City = city;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}


