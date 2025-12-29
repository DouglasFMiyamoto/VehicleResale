using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;

namespace InventoryService.Core.UseCases;

public sealed class ConfirmSaleUseCase : IConfirmSaleUseCase
{
    private readonly IReservationRepository _reservations;
    private readonly IVehicleRepository _vehicles;
    private readonly IClock _clock;

    public ConfirmSaleUseCase(IReservationRepository reservations, IVehicleRepository vehicles, IClock clock)
    {
        _reservations = reservations;
        _vehicles = vehicles;
        _clock = clock;
    }

    public async Task<bool> ExecuteAsync(string reservationId, CancellationToken ct)
    {
        var r = await _reservations.GetByIdAsync(reservationId, ct);
        if (r is null) return false;

        if (r.Status != ReservationStatus.Active)
            return true; // idempotente

        if (r.ExpiresAtUtc <= _clock.UtcNow)
        {
            r.MarkExpired();
            await _reservations.UpdateAsync(r, ct);
            await _vehicles.TryReleaseReservationAsync(r.VehicleId, r.ReservationId, ct);
            return false;
        }

        var ok = await _vehicles.TryMarkSoldAsync(r.VehicleId, r.ReservationId, ct);
        if (!ok) return false;

        r.MarkCompleted();
        await _reservations.UpdateAsync(r, ct);
        return true;
    }
}