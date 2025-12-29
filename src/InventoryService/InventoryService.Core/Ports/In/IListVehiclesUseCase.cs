using InventoryService.Core.Domain.Enums;

namespace InventoryService.Core.Ports.In;

public interface IListVehiclesUseCase
{
    Task<IReadOnlyList<VehicleListItem>> ExecuteAsync(VehicleStatus status, CancellationToken ct);
}

public sealed record VehicleListItem(
    string VehicleId,
    string Brand,
    string Model,
    int Year,
    string Color,
    long PriceCents,
    string Status
);