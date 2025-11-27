using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Tenants;

public interface ITenantService
{
    Task<IReadOnlyCollection<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken);
}

