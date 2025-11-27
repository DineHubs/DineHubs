using OrderManagement.Identity.Entities;

namespace OrderManagement.Application.Users;

public interface IUserService
{
    Task<ApplicationUser> CreateUserAsync(Guid tenantId, string email, string password, string role, Guid? branchId, IReadOnlyCollection<string> currentUserRoles, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(Guid tenantId, CancellationToken cancellationToken);
    void ValidateUserCreationRole(string requestedRole, IReadOnlyCollection<string> currentUserRoles);
}

public record UserDto(Guid Id, string Email, string Role, Guid? BranchId, bool IsActive);
