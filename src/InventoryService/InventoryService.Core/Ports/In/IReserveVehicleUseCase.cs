namespace InventoryService.Core.Ports.In;

public interface IReserveVehicleUseCase
{
    Task<ReserveVehicleOutput> ExecuteAsync(ReserveVehicleInput input, CancellationToken ct);
}

public sealed record ReserveVehicleInput(string VehicleId, string CustomerId);

public sealed record ReserveVehicleOutput(
    bool Success,
    string? ReservationId,
    DateTime? ExpiresAtUtc,
    string? Error
);