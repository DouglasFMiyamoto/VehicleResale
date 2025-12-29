using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;

namespace InventoryService.Core.UseCases;

public sealed class CancelReservationUseCase : ICancelReservationUseCase
{
    private readonly IReservationRepository _reservations;
    private readonly IVehicleRepository _vehicles;

    public CancelReservationUseCase(IReservationRepository reservations, IVehicleRepository vehicles)
    {
        _reservations = reservations;
        _vehicles = vehicles;
    }

    public async Task<bool> ExecuteAsync(string reservationId, CancellationToken ct)
    {
        var r = await _reservations.GetByIdAsync(reservationId, ct);
        if (r is null) return false;

        if (r.Status != ReservationStatus.Active)
            return true; // idempotente

        r.MarkCancelled();
        await _reservations.UpdateAsync(r, ct);

        await _vehicles.TryReleaseReservationAsync(r.VehicleId, r.ReservationId, ct);
        return true;
    }
}