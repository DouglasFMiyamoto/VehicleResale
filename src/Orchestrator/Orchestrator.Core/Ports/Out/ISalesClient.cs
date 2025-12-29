namespace Orchestrator.Core.Ports.Out;

public interface ISalesClient
{
    Task<CreateSaleResult> CreateAsync(string vehicleId, string customerId, string reservationId, long priceCents, CancellationToken ct);
}

public sealed record CreateSaleResult(string SaleId, bool AlreadyExisted);