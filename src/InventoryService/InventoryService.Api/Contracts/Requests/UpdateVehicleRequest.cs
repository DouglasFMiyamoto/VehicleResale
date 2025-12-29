namespace InventoryService.Api.Contracts.Requests;

public sealed record UpdateVehicleRequest(
    string Brand,
    string Model,
    int Year,
    string Color,
    long PriceCents
);