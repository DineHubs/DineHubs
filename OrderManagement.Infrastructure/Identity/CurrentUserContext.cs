using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrderManagement.Application.Abstractions;

namespace OrderManagement.Infrastructure.Identity;

public sealed class CurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    private readonly ClaimsPrincipal? _principal = accessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var id = _principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? Email => _principal?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles =>
        _principal?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? Array.Empty<string>();

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}


