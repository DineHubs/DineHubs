using OrderManagement.Identity.Entities;

namespace OrderManagement.Application.Users;

public interface IUserService
{
    Task<ApplicationUser> CreateUserAsync(Guid tenantId, string email, string password, string role, Guid? branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync(Guid tenantId, CancellationToken cancellationToken);
}

public record UserDto(Guid Id, string Email, string Role, Guid? BranchId, bool IsActive);
