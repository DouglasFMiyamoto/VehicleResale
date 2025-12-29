namespace Orchestrator.Core.Ports.Out;

public interface IInventoryClient
{
    Task<ReserveResult> ReserveAsync(string vehicleId, string customerId, CancellationToken ct);
    Task CancelReservationAsync(string reservationId, CancellationToken ct);
    Task<bool> ConfirmSaleAsync(string reservationId, CancellationToken ct);
}

public sealed record ReserveResult(bool Success, string? ReservationId, DateTime? ExpiresAtUtc, string? Error);