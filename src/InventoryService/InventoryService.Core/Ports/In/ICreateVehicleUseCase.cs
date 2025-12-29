namespace InventoryService.Core.Ports.In;

public interface ICreateVehicleUseCase
{
    Task<string> ExecuteAsync(CreateVehicleInput input, CancellationToken ct);
}

public sealed record CreateVehicleInput(string Brand, string Model, int Year, string Color, long PriceCents);