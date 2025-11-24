using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Tenants;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Tenancy;

public sealed class TenantProvisioningService(
    OrderManagementDbContext dbContext,
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

            var tenant = new Tenant(name, code);
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Created tenant {TenantCode} (Id: {TenantId}, Name: {TenantName}) for admin {Admin}", 
                code, tenant.Id, name, adminEmail);
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


