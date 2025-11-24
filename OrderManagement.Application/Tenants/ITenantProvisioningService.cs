using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Tenants;

public interface ITenantProvisioningService
{
    Task<Tenant> CreateTenantAsync(string name, string code, string adminEmail, CancellationToken cancellationToken);
}


