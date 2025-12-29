namespace SalesService.Core.Ports.In;

public interface IListSalesUseCase
{
    Task<IReadOnlyList<SaleListItem>> ExecuteAsync(CancellationToken ct);
}

public sealed record SaleListItem(
    string SaleId,
    string VehicleId,
    string CustomerId,
    string ReservationId,
    long PriceCents,
    DateTime SoldAtUtc
);