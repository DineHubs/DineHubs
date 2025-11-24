namespace OrderManagement.Application.Abstractions;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}


