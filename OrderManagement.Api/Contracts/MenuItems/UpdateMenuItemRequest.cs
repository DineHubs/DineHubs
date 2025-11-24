namespace OrderManagement.Api.Contracts.MenuItems;

public sealed record UpdateMenuItemRequest(
    string Name,
    string Category,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl = null);

