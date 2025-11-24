using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class MenuItem : BranchScopedEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public string? ImageUrl { get; private set; }

    private MenuItem()
    {
    }

    public MenuItem(Guid tenantId, Guid branchId, string name, string category, decimal price, string? imageUrl = null)
        : base(tenantId, branchId)
    {
        Name = name;
        Category = category;
        Price = price;
        ImageUrl = imageUrl;
    }

    public void UpdateDetails(string name, string category)
    {
        Name = name;
        Category = category;
    }

    public void UpdatePrice(decimal price) => Price = price;

    public void ToggleAvailability(bool isAvailable) => IsAvailable = isAvailable;

    public void UpdateImageUrl(string? imageUrl) => ImageUrl = imageUrl;
}


