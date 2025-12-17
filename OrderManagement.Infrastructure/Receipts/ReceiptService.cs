using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Ordering;
using OrderManagement.Application.Payments;
using OrderManagement.Application.Receipts;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Receipts;

public sealed class ReceiptService(
    OrderManagementDbContext dbContext,
    IOrderService orderService,
    IPaymentService paymentService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IReceiptService
{
    public async Task<string> GenerateReceiptAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(
                orderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order is null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            var payment = await paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
            if (payment is null)
            {
                throw new InvalidOperationException("Payment not found for order.");
            }

            // Generate receipt URL (in production, this would generate a PDF)
            var receiptUrl = $"receipts/{orderId}/{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";

            // Update payment with receipt URL if not already set
            if (string.IsNullOrEmpty(payment.ReceiptUrl))
            {
                payment.MarkCaptured(receiptUrl);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Create receipt print record
            var receiptPrint = new ReceiptPrint(
                tenantContext.TenantId,
                tenantContext.BranchId ?? Guid.Empty,
                orderId,
                payment.Id,
                receiptUrl,
                "Initial receipt generation",
                isReprint: false);

            dbContext.ReceiptPrints.Add(receiptPrint);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.Information("Generated receipt for order {OrderId} at {ReceiptUrl} for tenant {TenantId}, branch {BranchId}",
                orderId, receiptUrl, tenantContext.TenantId, tenantContext.BranchId);

            return receiptUrl;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error generating receipt for order {OrderId}", orderId);
            throw new InvalidOperationException("An error occurred while generating the receipt.");
        }
    }

    public async Task<string> ReprintReceiptAsync(
        Guid orderId,
        string reason,
        Guid? printedBy,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(
                orderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order is null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            var payment = await paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
            if (payment is null)
            {
                throw new InvalidOperationException("Payment not found for order.");
            }

            // Check reprint count
            var reprintCount = await dbContext.ReceiptPrints
                .CountAsync(r => r.OrderId == orderId && r.IsReprint, cancellationToken);

            // Generate new receipt URL with timestamp
            var receiptUrl = $"receipts/{orderId}/reprint-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";

            // Create receipt print record
            var receiptPrint = new ReceiptPrint(
                tenantContext.TenantId,
                tenantContext.BranchId ?? Guid.Empty,
                orderId,
                payment.Id,
                receiptUrl,
                reason,
                isReprint: true,
                printedBy);

            dbContext.ReceiptPrints.Add(receiptPrint);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.Information("Reprinted receipt for order {OrderId} (reprint #{Count}) with reason: {Reason} for tenant {TenantId}, branch {BranchId}",
                orderId, reprintCount + 1, reason, tenantContext.TenantId, tenantContext.BranchId);

            return receiptUrl;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error reprinting receipt for order {OrderId}", orderId);
            throw new InvalidOperationException("An error occurred while reprinting the receipt.");
        }
    }

    public async Task<string?> GetReceiptUrlAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
            return payment?.ReceiptUrl;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving receipt URL for order {OrderId}", orderId);
            throw new InvalidOperationException("An error occurred while retrieving the receipt URL.");
        }
    }
}

