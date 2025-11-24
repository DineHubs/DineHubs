namespace OrderManagement.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    bool IsTakeAway,
    string TableNumber,
    IReadOnlyCollection<CreateOrderLineRequest> Items);

public sealed record CreateOrderLineRequest(
    Guid MenuItemId,
    string Name,
    decimal Price,
    int Quantity);


