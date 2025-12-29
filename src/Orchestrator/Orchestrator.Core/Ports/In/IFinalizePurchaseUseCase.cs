namespace Orchestrator.Core.Ports.In;

public interface IFinalizePurchaseUseCase
{
    Task<FinalizePurchaseResult> ExecuteAsync(FinalizePurchaseCommand cmd, CancellationToken ct);
}

public sealed record FinalizePurchaseCommand(
    string VehicleId,
    string CustomerId,
    string ReservationId,
    string PaymentId,
    long PriceCents
);

public sealed record FinalizePurchaseResult(
    string SaleId,
    string Status
);