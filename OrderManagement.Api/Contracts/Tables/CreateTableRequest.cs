namespace OrderManagement.Api.Contracts.Tables;

public record CreateTableRequest(
    Guid BranchId,
    string TableNumber,
    double PositionX = 0,
    double PositionY = 0,
    double Width = 100,
    double Height = 100);

