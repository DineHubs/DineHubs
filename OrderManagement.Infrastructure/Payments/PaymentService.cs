using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Ordering;
using OrderManagement.Application.Payments;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;
using PaymentTiming = OrderManagement.Domain.Entities.PaymentTiming;

namespace OrderManagement.Infrastructure.Payments;

public sealed class PaymentService(
    OrderManagementDbContext dbContext,
    IPaymentGatewayFactory gatewayFactory,
    IOrderService orderService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IPaymentService
{
    public async Task<PaymentTransaction> ProcessPaymentAsync(
        Guid orderId,
        decimal amount,
        string provider,
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get order to verify it exists and belongs to tenant
            var order = await orderService.GetOrderByIdAsync(
                orderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order is null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            // Check if payment already exists
            var existingPayment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.TenantId == tenantContext.TenantId, cancellationToken);

            if (existingPayment != null && existingPayment.Status == PaymentStatus.Captured)
            {
                throw new InvalidOperationException("Order already has a captured payment.");
            }

            // Get payment gateway
            var gateway = gatewayFactory.GetGateway(provider);

            // Create payment through gateway
            var payment = await gateway.CreatePaymentAsync(
                orderId,
                amount,
                order.IsTakeAway ? "MYR" : "MYR", // Default currency
                metadata ?? new Dictionary<string, string>(),
                cancellationToken);

            // Capture payment (in real implementation, this would be done after gateway confirmation)
            // For now, we'll mark it as captured immediately
            payment.MarkCaptured($"receipt-{orderId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf");

            await dbContext.SaveChangesAsync(cancellationToken);

            // Update order status to Paid using the service method for proper EF Core tracking
            await orderService.UpdateOrderStatusAsync(
                orderId,
                OrderStatus.Paid,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            logger.Information("Processed payment {PaymentId} for order {OrderId} via {Provider} for tenant {TenantId}, branch {BranchId}",
                payment.Id, orderId, provider, tenantContext.TenantId, tenantContext.BranchId);

            return payment;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error processing payment for order {OrderId}", orderId);
            throw new InvalidOperationException("An error occurred while processing the payment.");
        }
    }

    public async Task<PaymentTransaction> RefundPaymentAsync(
        Guid paymentId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.TenantId == tenantContext.TenantId, cancellationToken);

            if (payment is null)
            {
                throw new InvalidOperationException("Payment not found.");
            }

            if (payment.Status != PaymentStatus.Captured)
            {
                throw new InvalidOperationException("Only captured payments can be refunded.");
            }

            if (amount > payment.Amount)
            {
                throw new InvalidOperationException("Refund amount cannot exceed payment amount.");
            }

            // Process refund through gateway (in real implementation)
            // For now, we'll just update the status
            payment.Refund(reason);

            await dbContext.SaveChangesAsync(cancellationToken);

            // If full refund, update order status using proper service method for EF Core tracking
            if (amount >= payment.Amount)
            {
                var order = await orderService.GetOrderByIdAsync(
                    payment.OrderId,
                    tenantContext.TenantId,
                    tenantContext.BranchId,
                    cancellationToken);

                if (order != null && order.Status == OrderStatus.Paid)
                {
                    await orderService.UpdateOrderStatusAsync(
                        payment.OrderId,
                        OrderStatus.Cancelled,
                        tenantContext.TenantId,
                        tenantContext.BranchId,
                        cancellationToken);
                }
            }

            logger.Information("Refunded payment {PaymentId} for order {OrderId} with reason: {Reason} for tenant {TenantId}, branch {BranchId}",
                paymentId, payment.OrderId, reason, tenantContext.TenantId, tenantContext.BranchId);

            return payment;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error refunding payment {PaymentId}", paymentId);
            throw new InvalidOperationException("An error occurred while processing the refund.");
        }
    }

    public async Task<PaymentTransaction> VoidPaymentAsync(
        Guid paymentId,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.TenantId == tenantContext.TenantId, cancellationToken);

            if (payment is null)
            {
                throw new InvalidOperationException("Payment not found.");
            }

            // Void payment
            payment.Void(reason);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Update order status if needed using proper service method for EF Core tracking
            var order = await orderService.GetOrderByIdAsync(
                payment.OrderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order != null && order.Status == OrderStatus.Paid)
            {
                // Revert order status based on payment timing
                var newStatus = order.PaymentTiming == PaymentTiming.PayBeforeKitchen
                    ? OrderStatus.Submitted
                    : OrderStatus.Ready;

                await orderService.UpdateOrderStatusAsync(
                    payment.OrderId,
                    newStatus,
                    tenantContext.TenantId,
                    tenantContext.BranchId,
                    cancellationToken);
            }

            logger.Information("Voided payment {PaymentId} for order {OrderId} with reason: {Reason} for tenant {TenantId}, branch {BranchId}",
                paymentId, payment.OrderId, reason, tenantContext.TenantId, tenantContext.BranchId);

            return payment;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error voiding payment {PaymentId}", paymentId);
            throw new InvalidOperationException("An error occurred while voiding the payment.");
        }
    }

    public async Task<PaymentTransaction?> GetPaymentByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.OrderId == orderId && p.TenantId == tenantContext.TenantId,
                    cancellationToken);

            return payment;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving payment for order {OrderId}", orderId);
            throw new InvalidOperationException("An error occurred while retrieving the payment.");
        }
    }
}

