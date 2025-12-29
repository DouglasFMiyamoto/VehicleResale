using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;

namespace InventoryService.Core.UseCases;

public sealed class CreateVehicleUseCase : ICreateVehicleUseCase
{
    private readonly IVehicleRepository _repo;

    public CreateVehicleUseCase(IVehicleRepository repo) => _repo = repo;

    public async Task<string> ExecuteAsync(CreateVehicleInput input, CancellationToken ct)
    {
        var v = Vehicle.Create(input.Brand, input.Model, input.Year, input.Color, input.PriceCents);
        await _repo.CreateAsync(v, ct);
        return v.VehicleId;
    }
}