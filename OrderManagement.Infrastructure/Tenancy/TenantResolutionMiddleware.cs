using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Infrastructure.Options;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Tenancy;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly MultiTenancyOptions _options;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        IOptions<MultiTenancyOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();
            var dbContext = context.RequestServices.GetRequiredService<OrderManagementDbContext>();
            var tenantResolved = false;

            if (_options.Strategy.Equals("Header", StringComparison.OrdinalIgnoreCase))
            {
                var tenantCode = context.Request.Headers[_options.HeaderName].FirstOrDefault();
                var branchCode = context.Request.Headers[_options.BranchHeaderName].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(tenantCode))
                {
                    var tenant = await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Code == tenantCode);
                    if (tenant is not null)
                    {
                        (tenantContext as TenantContext)?.SetTenant(tenant.Id, tenant.Code);

                        if (!string.IsNullOrWhiteSpace(branchCode))
                        {
                            var branch = await dbContext.Branches.AsNoTracking()
                                .FirstOrDefaultAsync(b => b.Code == branchCode && b.TenantId == tenant.Id);
                            (tenantContext as TenantContext)?.SetBranch(branch?.Id, branch?.Code);
                        }
                        
                        tenantResolved = true;
                        Log.Debug("Tenant resolved from HTTP header: {TenantCode} (Id: {TenantId})", tenantCode, tenant.Id);
                    }
                    else
                    {
                        Log.Warning("Tenant with code {TenantCode} not found in database", tenantCode);
                    }
                }
            }

            // If tenant not resolved from headers, try to extract from JWT token claims
            if (!tenantResolved && context.User?.Identity?.IsAuthenticated == true)
            {
                var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
                var branchIdClaim = context.User.FindFirst("branchId")?.Value;

                if (!string.IsNullOrWhiteSpace(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    var tenant = await dbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tenantId);
                    if (tenant is not null)
                    {
                        (tenantContext as TenantContext)?.SetTenant(tenant.Id, tenant.Code);
                        tenantResolved = true;
                        Log.Debug("Tenant resolved from JWT claim: {TenantCode} (Id: {TenantId})", tenant.Code, tenant.Id);

                        // Set branch from JWT claim if present
                        if (!string.IsNullOrWhiteSpace(branchIdClaim) && Guid.TryParse(branchIdClaim, out var branchId))
                        {
                            var branch = await dbContext.Branches.AsNoTracking()
                                .FirstOrDefaultAsync(b => b.Id == branchId && b.TenantId == tenant.Id);
                            if (branch is not null)
                            {
                                (tenantContext as TenantContext)?.SetBranch(branch.Id, branch.Code);
                                Log.Debug("Branch resolved from JWT claim: {BranchCode} (Id: {BranchId})", branch.Code, branch.Id);
                            }
                        }
                    }
                    else
                    {
                        Log.Warning("Tenant with ID {TenantId} from JWT claim not found in database", tenantId);
                    }
                }
            }

            // Fallback to default tenant if configured and no tenant resolved yet
            if (!tenantResolved && _options.DefaultTenantId != Guid.Empty)
            {
                (tenantContext as TenantContext)?.SetTenant(_options.DefaultTenantId, null);
                Log.Debug("Using default tenant ID: {TenantId}", _options.DefaultTenantId);
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in TenantResolutionMiddleware while resolving tenant for request {Path}", context.Request.Path);
            await _next(context); // Continue pipeline even on error
        }
    }
}

