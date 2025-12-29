namespace SalesService.Core.Ports.In;

public interface ICreateSaleUseCase
{
    Task<CreateSaleResult> ExecuteAsync(CreateSaleCommand cmd, CancellationToken ct);
}

public sealed record CreateSaleCommand(
    string VehicleId,
    string CustomerId,
    string ReservationId,
    long PriceCents
);

public sealed record CreateSaleResult(
    string SaleId,
    bool AlreadyExisted
);