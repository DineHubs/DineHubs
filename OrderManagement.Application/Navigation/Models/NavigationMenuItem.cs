namespace OrderManagement.Application.Navigation.Models;

public sealed record NavigationMenuItem(
    string Id,
    string Label,
    string? Icon,
    string? Route,
    string? ParentId,
    IReadOnlyCollection<string> AllowedRoles,
    IReadOnlyCollection<NavigationMenuItem>? Children = null);

