namespace InventoryService.Core.Ports.In;

public interface ICancelReservationUseCase
{
    Task<bool> ExecuteAsync(string reservationId, CancellationToken ct);
}