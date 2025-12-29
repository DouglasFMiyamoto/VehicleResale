using InventoryService.Core.Domain.Entities;

namespace InventoryService.Core.Ports.Out;

public interface IReservationRepository
{
    Task CreateAsync(Reservation r, CancellationToken ct);
    Task<Reservation?> GetByIdAsync(string reservationId, CancellationToken ct);
    Task UpdateAsync(Reservation r, CancellationToken ct);
}