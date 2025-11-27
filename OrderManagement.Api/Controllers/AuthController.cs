using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Serilog;
using OrderManagement.Api.Contracts.Auth;
using OrderManagement.Application.Auth;
using OrderManagement.Application.Abstractions.Notifications;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    IEmailNotificationService emailNotificationService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.TenantId, user.BranchId, roles);
            return Ok(new { AccessToken = accessToken, Roles = roles });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to log in user {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to process login at this time.");
        }
    }

    [HttpPost("seed-super-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedSuperAdmin([FromBody] LoginRequest request)
    {
        try
        {
            var existing = await userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
            {
                return Conflict("Super admin already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                TenantId = Guid.Empty,
                DisplayName = "Super Admin"
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            await userManager.AddToRoleAsync(user, SystemRoles.SuperAdmin);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to seed super admin for email {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to seed super admin at this time.");
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                // Don't reveal if user exists for security reasons
                logger.Warning("Password reset requested for non-existent email: {Email}", request.Email);
                return Ok(new { Message = "If the email exists, a password reset link has been sent." });
            }

            // Generate password reset token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create reset link (in production, this should be a proper URL)
            var resetLink = $"/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";
            
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

            await emailNotificationService.SendAsync(user.Email!, subject, body);
            
            logger.Information("Password reset token generated and email sent to {Email}", request.Email);
            return Ok(new { Message = "If the email exists, a password reset link has been sent." });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to process forgot password request for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to process password reset request at this time.");
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                logger.Warning("Password reset attempted for non-existent email: {Email}", request.Email);
                return BadRequest(new { Message = "Invalid email or token." });
            }

            // Reset password using token
            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            
            if (!result.Succeeded)
            {
                logger.Warning("Password reset failed for {Email}: {Errors}", 
                    request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return BadRequest(new { Message = "Invalid or expired token.", Errors = result.Errors });
            }

            logger.Information("Password successfully reset for user {Email}", request.Email);
            return Ok(new { Message = "Password has been reset successfully." });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to reset password for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to reset password at this time.");
        }
    }
}

