namespace OrderManagement.Api.Contracts.MenuManagement;

public sealed record UpdateMenuPermissionsRequest(
    IReadOnlyCollection<string> AllowedRoles);

