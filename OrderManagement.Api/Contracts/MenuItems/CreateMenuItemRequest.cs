namespace OrderManagement.Api.Contracts.MenuItems;

public sealed record CreateMenuItemRequest(
    Guid BranchId,
    string Name,
    string Category,
    decimal Price,
    bool IsAvailable = true,
    string? ImageUrl = null);

