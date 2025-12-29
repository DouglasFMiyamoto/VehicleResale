using SalesService.Core.Domain.Entities;
using SalesService.Core.Ports.In;
using SalesService.Core.Ports.Out;

namespace SalesService.Core.UseCases;

public sealed class GetSaleUseCase : IGetSaleUseCase
{
    private readonly ISaleRepository _repo;
    public GetSaleUseCase(ISaleRepository repo) => _repo = repo;

    public Task<Sale?> ExecuteAsync(string saleId, CancellationToken ct)
        => _repo.GetByIdAsync(saleId, ct);
}