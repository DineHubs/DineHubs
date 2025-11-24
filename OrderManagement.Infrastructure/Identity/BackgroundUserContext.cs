using OrderManagement.Application.Abstractions;

namespace OrderManagement.Infrastructure.Identity;

public sealed class BackgroundUserContext : ICurrentUserContext
{
    public Guid? UserId => null;
    public string? Email => "system@ordermanagement.local";
    public IReadOnlyCollection<string> Roles { get; } = Array.Empty<string>();
    public bool IsInRole(string role) => false;
}


