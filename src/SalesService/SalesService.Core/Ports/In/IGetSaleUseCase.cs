using SalesService.Core.Domain.Entities;

namespace SalesService.Core.Ports.In;

public interface IGetSaleUseCase
{
    Task<Sale?> ExecuteAsync(string saleId, CancellationToken ct);
}