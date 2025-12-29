using InventoryService.Core.Domain.Enums;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;

namespace InventoryService.Core.UseCases;

public sealed class ListVehiclesUseCase : IListVehiclesUseCase
{
    private readonly IVehicleRepository _repo;

    public ListVehiclesUseCase(IVehicleRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<VehicleListItem>> ExecuteAsync(VehicleStatus status, CancellationToken ct)
    {
        var list = await _repo.ListByStatusAsync(status, ct);

        // ordenação asc por preço (POC: em memória)
        var ordered = list.OrderBy(v => v.PriceCents).ToList();

        return ordered.Select(v => new VehicleListItem(
            v.VehicleId, v.Brand, v.Model, v.Year, v.Color, v.PriceCents, v.Status.ToString()
        )).ToList();
    }
}