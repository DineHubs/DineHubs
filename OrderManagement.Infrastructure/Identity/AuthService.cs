using Microsoft.AspNetCore.Identity;
using Serilog;
using OrderManagement.Application.Auth;
using OrderManagement.Application.Auth.Models;
using OrderManagement.Application.Abstractions.Notifications;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;

namespace OrderManagement.Infrastructure.Identity;

public sealed class AuthService(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    IEmailNotificationService emailNotificationService,
    Serilog.ILogger logger) : IAuthService
{
    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.Warning("Login attempt with non-existent email: {Email}", email);
                throw new InvalidOperationException("Invalid email or password.");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                logger.Warning("Failed login attempt for {Email}: {Result}", email, result);
                throw new InvalidOperationException("Invalid email or password.");
            }

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.TenantId, user.BranchId, roles);
            
            logger.Information("User {Email} logged in successfully", email);
            return new LoginResult(accessToken, roles.ToList());
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to log in user {Email}", email);
            throw new InvalidOperationException("Unable to process login at this time.");
        }
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                // Don't reveal if user exists for security reasons
                logger.Warning("Password reset requested for non-existent email: {Email}", email);
                return new ForgotPasswordResult("If the email exists, a password reset link has been sent.");
            }

            // Generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create reset link (in production, this should be a proper URL)
            var resetLink = $"/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            
            // Send email with reset link
            var subject = "Password Reset Request";
            var body = $@"
Hello,

You have requested to reset your password for your account.

Please click the following link to reset your password:
{resetLink}

If you did not request this password reset, please ignore this email.

This link will expire in 24 hours.

Best regards,
Order Management System";

            await emailNotificationService.SendAsync(user.Email!, subject, body, cancellationToken);
            
            logger.Information("Password reset token generated and email sent to {Email}", email);
            return new ForgotPasswordResult("If the email exists, a password reset link has been sent.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to process forgot password request for {Email}", email);
            throw new InvalidOperationException("Unable to process password reset request at this time.");
        }
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                logger.Warning("Password reset attempted for non-existent email: {Email}", email);
                return new ResetPasswordResult(false, "Invalid email or token.");
            }

            // Reset password using token
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (!result.Succeeded)
            {
                logger.Warning("Password reset failed for {Email}: {Errors}", 
                    email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return new ResetPasswordResult(false, $"Invalid or expired token. {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            logger.Information("Password successfully reset for user {Email}", email);
            return new ResetPasswordResult(true, "Password has been reset successfully.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to reset password for {Email}", email);
            throw new InvalidOperationException("Unable to reset password at this time.");
        }
    }

    public async Task SeedSuperAdminAsync(string email, string password, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                logger.Warning("Attempted to seed super admin with existing email: {Email}", email);
                throw new InvalidOperationException("Super admin already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                TenantId = Guid.Empty,
                DisplayName = "Super Admin"
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.Error("Failed to create super admin user: {Errors}", errors);
                throw new InvalidOperationException($"Failed to create super admin: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, SystemRoles.SuperAdmin);
            if (!roleResult.Succeeded)
            {
                // Cleanup user if role assignment fails
                await userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.Error("Failed to assign SuperAdmin role: {Errors}", errors);
                throw new InvalidOperationException($"Failed to assign SuperAdmin role: {errors}");
            }

            logger.Information("Successfully seeded super admin with email {Email}", email);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to seed super admin for email {Email}", email);
            throw new InvalidOperationException("Unable to seed super admin at this time.");
        }
    }
}

