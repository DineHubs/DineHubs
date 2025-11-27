using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Tenants;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Tenancy;

public sealed class TenantService(
    OrderManagementDbContext dbContext,
    Serilog.ILogger logger) : ITenantService
{
    public async Task<IReadOnlyCollection<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await dbContext.Tenants
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved {Count} tenants", tenants.Count);
            return tenants;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving tenants");
            throw new InvalidOperationException("An error occurred while retrieving tenants.");
        }
    }
}

