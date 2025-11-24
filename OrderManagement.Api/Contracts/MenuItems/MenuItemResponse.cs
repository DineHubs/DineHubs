namespace OrderManagement.Api.Contracts.MenuItems;

public sealed record MenuItemResponse(
    Guid Id,
    Guid BranchId,
    string Name,
    string Category,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl);

