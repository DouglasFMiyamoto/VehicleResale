namespace InventoryService.Core.Ports.In;

public interface IConfirmSaleUseCase
{
    Task<bool> ExecuteAsync(string reservationId, CancellationToken ct);
}