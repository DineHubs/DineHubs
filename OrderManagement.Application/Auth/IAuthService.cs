using OrderManagement.Application.Auth.Models;

namespace OrderManagement.Application.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<ForgotPasswordResult> ForgotPasswordAsync(string email, CancellationToken cancellationToken);
    Task<ResetPasswordResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);
    Task SeedSuperAdminAsync(string email, string password, CancellationToken cancellationToken);
}

