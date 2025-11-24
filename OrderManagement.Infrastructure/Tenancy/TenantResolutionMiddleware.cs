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
                    }
                    else
                    {
                        Log.Warning("Tenant with code {TenantCode} not found in database", tenantCode);
                    }
                }
                else if (_options.DefaultTenantId != Guid.Empty)
                {
                    (tenantContext as TenantContext)?.SetTenant(_options.DefaultTenantId, null);
                }
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

