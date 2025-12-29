using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Domain.Enums;

namespace InventoryService.Core.Ports.Out;

public interface IVehicleRepository
{
    Task CreateAsync(Vehicle vehicle, CancellationToken ct);
    Task<Vehicle?> GetByIdAsync(string vehicleId, CancellationToken ct);
    Task UpdateAsync(Vehicle vehicle, CancellationToken ct);
    Task<IReadOnlyList<Vehicle>> ListByStatusAsync(VehicleStatus status, CancellationToken ct);

    Task<bool> TryReserveAsync(string vehicleId, string reservationId, string customerId, DateTime expiresAtUtc, CancellationToken ct);
    
    /// <summary>
    /// Libera veículo (RESERVED -> AVAILABLE) apenas se ReservationId bater.
    /// </summary>
    Task<bool> TryReleaseReservationAsync(string vehicleId, string reservationId, CancellationToken ct);

    /// <summary>
    /// Confirma venda (RESERVED -> SOLD) apenas se ReservationId bater e não estiver expirado.
    /// </summary>
    Task<bool> TryMarkSoldAsync(string vehicleId, string reservationId, CancellationToken ct);
}