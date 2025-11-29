using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Navigation;
using OrderManagement.Application.Tenants;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Tenancy;

public sealed class TenantProvisioningService(
    OrderManagementDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    INavigationMenuService navigationMenuService,
    Serilog.ILogger logger) : ITenantProvisioningService
{
    public async Task<Tenant> CreateTenantAsync(string name, string code, string adminEmail, CancellationToken cancellationToken)
    {
        try
        {
            if (await dbContext.Tenants.AnyAsync(t => t.Code == code, cancellationToken))
            {
                logger.Warning("Attempted to create tenant with existing code {Code}", code);
                throw new InvalidOperationException($"Tenant code {code} already exists.");
            }

            // 1. Create Tenant
            var tenant = new Tenant(name, code);
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Created tenant {TenantCode} (Id: {TenantId}, Name: {TenantName})", 
                code, tenant.Id, name);

            // 2. Create Main Branch
            var mainBranch = new Branch(tenant.Id, "Main Branch", "main", string.Empty, string.Empty);
            dbContext.Branches.Add(mainBranch);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Created main branch for tenant {TenantId}", tenant.Id);

            // 3. Create Admin User
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                TenantId = tenant.Id,
                BranchId = mainBranch.Id,
                DisplayName = adminEmail.Split('@')[0],
                EmailConfirmed = false // Will need email confirmation for password reset
            };

            // Use default password for new tenant admin
            const string defaultPassword = "P@$$w0rd";
            var createResult = await userManager.CreateAsync(adminUser, defaultPassword);
            
            if (!createResult.Succeeded)
            {
                logger.Error("Failed to create admin user for tenant {TenantId}: {Errors}", 
                    tenant.Id, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            // 4. Assign Admin Role
            var roleResult = await userManager.AddToRoleAsync(adminUser, SystemRoles.Admin);
            if (!roleResult.Succeeded)
            {
                logger.Error("Failed to assign Admin role to user {Email}: {Errors}", 
                    adminEmail, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException(
                    $"Failed to assign Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            // 5. Generate Password Reset Token (for forgot-password support)
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            logger.Information("Generated password reset token for admin user {Email} (tenant {TenantId})", 
                adminEmail, tenant.Id);
            // Note: The token is available but email sending should be handled separately
            // Store token or send email with reset link in production

            // 6. Seed Navigation Menu
            await navigationMenuService.SeedMenuForTenantAsync(tenant.Id, cancellationToken);
            logger.Information("Seeded navigation menu for tenant {TenantId}", tenant.Id);

            logger.Information("Tenant initialization completed for {TenantCode} (Id: {TenantId}, Admin: {Admin}) with default password", 
                code, tenant.Id, adminEmail);
            
            return tenant;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating tenant {Code} for admin {Admin}", code, adminEmail);
            throw;
        }
    }
}


