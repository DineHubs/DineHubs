namespace OrderManagement.Api.Contracts.Tables;

public record UpdateTableRequest(
    string TableNumber,
    double PositionX,
    double PositionY,
    double Width,
    double Height);

