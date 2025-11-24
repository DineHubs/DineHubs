namespace OrderManagement.Application.Abstractions;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid? BranchId { get; }
    string? TenantCode { get; }
    string? BranchCode { get; }
}


