using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class NavigationMenuItem : TenantScopedEntity
{
    public string Label { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public string? Route { get; private set; }
    public Guid? ParentId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation properties
    public NavigationMenuItem? Parent { get; private set; }
    private readonly List<NavigationMenuItem> _children = new();
    public IReadOnlyCollection<NavigationMenuItem> Children => _children;

    private readonly List<MenuPermission> _permissions = new();
    public IReadOnlyCollection<MenuPermission> Permissions => _permissions;

    private NavigationMenuItem()
    {
    }

    public NavigationMenuItem(Guid tenantId, string label, string? icon, string? route, Guid? parentId, int displayOrder)
        : base(tenantId)
    {
        Label = label;
        Icon = icon;
        Route = route;
        ParentId = parentId;
        DisplayOrder = displayOrder;
    }

    public void UpdateDetails(string label, string? icon, string? route)
    {
        Label = label;
        Icon = icon;
        Route = route;
    }

    public void UpdateDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;

    public void ToggleActive(bool isActive) => IsActive = isActive;

    public void AddPermission(MenuPermission permission) => _permissions.Add(permission);

    public void RemovePermission(MenuPermission permission) => _permissions.Remove(permission);

    public void ClearPermissions() => _permissions.Clear();
}

