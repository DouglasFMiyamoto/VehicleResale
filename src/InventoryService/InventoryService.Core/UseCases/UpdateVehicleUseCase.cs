using InventoryService.Core.Domain.Errors;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;

namespace InventoryService.Core.UseCases;

public sealed class UpdateVehicleUseCase : IUpdateVehicleUseCase
{
    private readonly IVehicleRepository _repo;

    public UpdateVehicleUseCase(IVehicleRepository repo) => _repo = repo;

    public async Task ExecuteAsync(UpdateVehicleInput input, CancellationToken ct)
    {
        var v = await _repo.GetByIdAsync(input.VehicleId, ct);
        if (v is null) throw new DomainException("Vehicle não encontrado.");

        v.Update(input.Brand, input.Model, input.Year, input.Color, input.PriceCents);
        await _repo.UpdateAsync(v, ct);
    }
}