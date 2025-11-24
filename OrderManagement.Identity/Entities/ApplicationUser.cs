using Microsoft.AspNetCore.Identity;

namespace OrderManagement.Identity.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}


