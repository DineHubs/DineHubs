using OrderManagement.Application.Tenants.Models;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Tenants;

public interface ITenantService
{
    Task<IReadOnlyCollection<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken);
    Task<TenantDetailDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken);
}

