using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Branches;

public interface IBranchService
{
    Task<Branch> CreateBranchAsync(Guid tenantId, string name, string location, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Branch>> GetBranchesAsync(Guid tenantId, CancellationToken cancellationToken);
}
