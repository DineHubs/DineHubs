using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class MenuPermission : BaseEntity
{
    public Guid MenuItemId { get; private set; }
    public string RoleName { get; private set; } = string.Empty;

    // Navigation property
    public NavigationMenuItem MenuItem { get; private set; } = null!;

    private MenuPermission()
    {
    }

    public MenuPermission(Guid menuItemId, string roleName)
    {
        MenuItemId = menuItemId;
        RoleName = roleName;
    }
}

