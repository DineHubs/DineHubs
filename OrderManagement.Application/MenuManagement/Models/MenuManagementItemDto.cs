namespace OrderManagement.Application.MenuManagement.Models;

public sealed record MenuManagementItemDto(
    Guid Id,
    string Label,
    string? Icon,
    string? Route,
    Guid? ParentId,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyCollection<string> AllowedRoles);

