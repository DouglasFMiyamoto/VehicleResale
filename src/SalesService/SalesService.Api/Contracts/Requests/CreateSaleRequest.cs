namespace SalesService.Api.Contracts.Requests;

public sealed record CreateSaleRequest(
    string VehicleId,
    string CustomerId,
    string ReservationId,
    long PriceCents
);