namespace InventoryService.Api.Contracts.Requests;

public sealed record CreateVehicleRequest(
    string Brand,
    string Model,
    int Year,
    string Color,
    long PriceCents
);