using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Serilog;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Auth;
using OrderManagement.Infrastructure.Options;

namespace OrderManagement.Infrastructure.Identity;

public sealed class JwtTokenGenerator(
    IOptions<JwtOptions> options,
    Serilog.ILogger logger) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;
    private readonly Serilog.ILogger _logger = logger;

    public string GenerateAccessToken(Guid userId, string email, Guid tenantId, Guid? branchId, IEnumerable<string> roles)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Email, email),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new("tenantId", tenantId.ToString())
            };

            if (branchId.HasValue)
            {
                claims.Add(new Claim("branchId", branchId.Value.ToString()));
            }

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var keyBytes = Encoding.UTF8.GetBytes(_options.Key);
            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to generate access token for user {UserId}", userId);
            throw;
        }
    }
}

