using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Domain.Errors;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;

namespace InventoryService.Core.UseCases;

public sealed class ReserveVehicleUseCase : IReserveVehicleUseCase
{
    private readonly IVehicleRepository _vehicles;
    private readonly IReservationRepository _reservations;
    private readonly IClock _clock;
    private readonly int _ttlMinutes;

    public ReserveVehicleUseCase(
        IVehicleRepository vehicles,
        IReservationRepository reservations,
        IClock clock,
        int ttlMinutes)
    {
        _vehicles = vehicles;
        _reservations = reservations;
        _clock = clock;
        _ttlMinutes = ttlMinutes;
    }

    public async Task<ReserveVehicleOutput> ExecuteAsync(ReserveVehicleInput input, CancellationToken ct)
    {
        var v = await _vehicles.GetByIdAsync(input.VehicleId, ct);
        if (v is null)
            return new ReserveVehicleOutput(false, null, null, "Vehicle não encontrado.");

        // Se estiver RESERVED mas expirado, libera (POC)
        if (v.IsReservationExpired(_clock.UtcNow) && v.ReservationId is not null)
        {
            await _vehicles.TryReleaseReservationAsync(v.VehicleId, v.ReservationId, ct);
            // segue fluxo de reservar normalmente
        }

        var r = Reservation.Create(input.VehicleId, input.CustomerId, _ttlMinutes);

        // tenta reservar com concorrência no Dynamo
        var reserved = await _vehicles.TryReserveAsync(input.VehicleId, r.ReservationId, input.CustomerId, r.ExpiresAtUtc, ct);
        if (!reserved)
            return new ReserveVehicleOutput(false, null, null, "Veículo indisponível (já reservado ou vendido).");

        await _reservations.CreateAsync(r, ct);

        return new ReserveVehicleOutput(true, r.ReservationId, r.ExpiresAtUtc, null);
    }
}