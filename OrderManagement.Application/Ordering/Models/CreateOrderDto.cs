namespace OrderManagement.Application.Ordering.Models;

public record CreateOrderDto(
    bool IsTakeAway,
    string? TableNumber,
    IReadOnlyCollection<OrderLineDto> Items);

public record OrderLineDto(
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity);

