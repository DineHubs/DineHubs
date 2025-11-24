namespace OrderManagement.Application.Auth;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(Guid userId, string email, Guid tenantId, Guid? branchId, IEnumerable<string> roles);
}

