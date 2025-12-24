namespace OrderManagement.Application.Kitchen;

/// <summary>
/// DTO representing a kitchen ticket for printing
/// </summary>
public record KitchenTicketDto(
    string OrderNumber,
    string TableNumber,
    bool IsTakeAway,
    DateTimeOffset OrderTime,
    IReadOnlyCollection<KitchenTicketItemDto> Items,
    string? Notes
);

/// <summary>
/// DTO for individual items on a kitchen ticket
/// </summary>
public record KitchenTicketItemDto(
    string Name,
    int Quantity,
    string? SpecialInstructions
);

/// <summary>
/// Result of a print operation
/// </summary>
public record KitchenPrintResult(
    bool Success,
    Guid? PrintJobId,
    string? Message,
    KitchenTicketDto? Ticket
);

/// <summary>
/// Service interface for kitchen ticket printing operations
/// </summary>
public interface IKitchenPrintService
{
    /// <summary>
    /// Generate kitchen ticket data for an order (for frontend printing)
    /// </summary>
    Task<KitchenTicketDto?> GenerateKitchenTicketAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Print kitchen ticket (generates ticket and logs print event)
    /// </summary>
    Task<KitchenPrintResult> PrintKitchenTicketAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reprint kitchen ticket with audit reason
    /// </summary>
    Task<KitchenPrintResult> ReprintKitchenTicketAsync(
        Guid orderId,
        string reason,
        CancellationToken cancellationToken);
}

