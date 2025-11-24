using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Application.Ordering;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Ordering;

public sealed class QrOrderingService(
    OrderManagementDbContext dbContext,
    ILogger<QrOrderingService> logger) : IQrOrderingService
{
    public async Task<string> GenerateSessionAsync(Guid tenantId, Guid branchId, string tableNumber, CancellationToken cancellationToken)
    {
        var code = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", string.Empty)
            .Replace("+", string.Empty)
            .Substring(0, 8);

        var session = new QrOrderSession(tenantId, branchId, code, tableNumber, DateTimeOffset.UtcNow.AddHours(3));
        dbContext.QrSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Generated QR session {Code} for tenant {Tenant}", code, tenantId);
        return code;
    }

    public async Task CloseSessionAsync(string sessionCode, CancellationToken cancellationToken)
    {
        var session = await dbContext.QrSessions.FirstOrDefaultAsync(s => s.SessionCode == sessionCode, cancellationToken);
        if (session is null)
        {
            return;
        }

        session.Close();
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

