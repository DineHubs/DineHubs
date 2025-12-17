namespace OrderManagement.Application.MenuItems.Models;

public sealed record MenuItemDto(
    Guid Id,
    Guid BranchId,
    string Name,
    string Category,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl);

public sealed record CreateMenuItemDto(
    Guid BranchId,
    string Name,
    string Category,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl);

public sealed record UpdateMenuItemDto(
    string Name,
    string Category,
    decimal Price,
    bool IsAvailable,
    string? ImageUrl);

