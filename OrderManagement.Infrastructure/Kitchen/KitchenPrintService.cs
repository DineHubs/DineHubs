using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Kitchen;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;
using Serilog;

namespace OrderManagement.Infrastructure.Kitchen;

public sealed class KitchenPrintService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IKitchenPrintService
{
    public async Task<KitchenTicketDto?> GenerateKitchenTicketAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.Id == orderId && o.TenantId == tenantContext.TenantId);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            var order = await query.FirstOrDefaultAsync(cancellationToken);
            if (order is null)
            {
                logger.Warning("Order {OrderId} not found for kitchen ticket generation", orderId);
                return null;
            }

            var ticket = new KitchenTicketDto(
                OrderNumber: order.OrderNumber,
                TableNumber: order.TableNumber,
                IsTakeAway: order.IsTakeAway,
                OrderTime: order.CreatedAt,
                Items: order.Lines.Select(l => new KitchenTicketItemDto(
                    Name: l.Name,
                    Quantity: l.Quantity,
                    SpecialInstructions: null // Could be added if OrderLine supports special instructions
                )).ToList(),
                Notes: null
            );

            logger.Information("Generated kitchen ticket for order {OrderNumber}", order.OrderNumber);
            return ticket;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error generating kitchen ticket for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<KitchenPrintResult> PrintKitchenTicketAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await GenerateKitchenTicketAsync(orderId, cancellationToken);
            if (ticket is null)
            {
                return new KitchenPrintResult(
                    Success: false,
                    PrintJobId: null,
                    Message: "Order not found",
                    Ticket: null
                );
            }

            // Generate a print job ID for tracking
            var printJobId = Guid.NewGuid();

            // Log the print event
            logger.Information(
                "Kitchen ticket printed for order {OrderNumber}. PrintJobId: {PrintJobId}, Items: {ItemCount}",
                ticket.OrderNumber,
                printJobId,
                ticket.Items.Count);

            // In a real implementation, this would send to a print queue or network printer
            // For now, we return the ticket data for frontend printing
            return new KitchenPrintResult(
                Success: true,
                PrintJobId: printJobId,
                Message: "Kitchen ticket generated successfully",
                Ticket: ticket
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error printing kitchen ticket for order {OrderId}", orderId);
            return new KitchenPrintResult(
                Success: false,
                PrintJobId: null,
                Message: $"Failed to print kitchen ticket: {ex.Message}",
                Ticket: null
            );
        }
    }

    public async Task<KitchenPrintResult> ReprintKitchenTicketAsync(Guid orderId, string reason, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await GenerateKitchenTicketAsync(orderId, cancellationToken);
            if (ticket is null)
            {
                return new KitchenPrintResult(
                    Success: false,
                    PrintJobId: null,
                    Message: "Order not found",
                    Ticket: null
                );
            }

            // Generate a print job ID for tracking
            var printJobId = Guid.NewGuid();

            // Log the reprint event with reason for audit purposes
            logger.Information(
                "Kitchen ticket REPRINTED for order {OrderNumber}. PrintJobId: {PrintJobId}, Reason: {Reason}, Items: {ItemCount}",
                ticket.OrderNumber,
                printJobId,
                reason,
                ticket.Items.Count);

            return new KitchenPrintResult(
                Success: true,
                PrintJobId: printJobId,
                Message: "Kitchen ticket reprinted successfully",
                Ticket: ticket
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error reprinting kitchen ticket for order {OrderId}", orderId);
            return new KitchenPrintResult(
                Success: false,
                PrintJobId: null,
                Message: $"Failed to reprint kitchen ticket: {ex.Message}",
                Ticket: null
            );
        }
    }
}

