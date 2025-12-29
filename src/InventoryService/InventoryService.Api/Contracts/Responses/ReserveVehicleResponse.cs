namespace InventoryService.Api.Contracts.Responses;

public sealed record ReserveVehicleResponse(
    bool Success,
    string? ReservationId,
    DateTime? ExpiresAtUtc,
    string? Error
);