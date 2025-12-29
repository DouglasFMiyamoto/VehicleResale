using SalesService.Core.Domain.Entities;
using SalesService.Core.Ports.In;
using SalesService.Core.Ports.Out;

namespace SalesService.Core.UseCases;

public sealed class CreateSaleUseCase : ICreateSaleUseCase
{
    private readonly ISaleRepository _repo;

    public CreateSaleUseCase(ISaleRepository repo) => _repo = repo;

    public async Task<CreateSaleResult> ExecuteAsync(CreateSaleCommand cmd, CancellationToken ct)
    {
        // Idempotência: se a SAGA chamar 2x por retry, não duplica.
        var existing = await _repo.GetByReservationIdAsync(cmd.ReservationId, ct);
        if (existing is not null)
            return new CreateSaleResult(existing.SaleId, AlreadyExisted: true);

        var sale = Sale.Create(cmd.VehicleId, cmd.CustomerId, cmd.ReservationId, cmd.PriceCents);
        await _repo.CreateAsync(sale, ct);
        return new CreateSaleResult(sale.SaleId, AlreadyExisted: false);
    }
}