namespace InventoryService.Core.Ports.In;

public interface IUpdateVehicleUseCase
{
    Task ExecuteAsync(UpdateVehicleInput input, CancellationToken ct);
}

public sealed record UpdateVehicleInput(string VehicleId, string Brand, string Model, int Year, string Color, long PriceCents);