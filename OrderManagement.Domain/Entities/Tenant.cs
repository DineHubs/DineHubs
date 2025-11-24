using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string CountryCode { get; private set; } = "MY";
    public string DefaultCurrency { get; private set; } = "MYR";
    public bool IsActive { get; private set; } = true;

    private readonly List<Branch> _branches = new();
    public IReadOnlyCollection<Branch> Branches => _branches;

    private Tenant()
    {
    }

    public Tenant(string name, string code, string countryCode = "MY", string currency = "MYR")
    {
        Name = name;
        Code = code;
        CountryCode = countryCode;
        DefaultCurrency = currency;
    }

    public void UpdateProfile(string name, string currency)
    {
        Name = name;
        DefaultCurrency = currency;
    }

    public void AddBranch(Branch branch) => _branches.Add(branch);

    public void Activate() => IsActive = true;

    public void Suspend() => IsActive = false;
}


