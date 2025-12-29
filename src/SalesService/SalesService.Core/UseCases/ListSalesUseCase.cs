using SalesService.Core.Ports.In;
using SalesService.Core.Ports.Out;

namespace SalesService.Core.UseCases;

public sealed class ListSalesUseCase : IListSalesUseCase
{
    private readonly ISaleRepository _repo;
    public ListSalesUseCase(ISaleRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<SaleListItem>> ExecuteAsync(CancellationToken ct)
    {
        var list = await _repo.ListAllAsync(ct);

        // ordena por preço asc (mais barato->mais caro), como no requisito geral
        return list
            .OrderBy(s => s.PriceCents)
            .Select(s => new SaleListItem(s.SaleId, s.VehicleId, s.CustomerId, s.ReservationId, s.PriceCents, s.SoldAtUtc))
            .ToList();
    }
}