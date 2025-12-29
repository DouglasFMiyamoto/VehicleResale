namespace Orchestrator.Core.Ports.In;

public interface IStartPurchaseUseCase
{
    Task<StartPurchaseResult> ExecuteAsync(StartPurchaseCommand cmd, CancellationToken ct);
}

public sealed record StartPurchaseCommand(string VehicleId, string CustomerId, long PriceCents);

public sealed record StartPurchaseResult(
    string VehicleId,
    string CustomerId,
    string ReservationId,
    string PaymentId,
    string PaymentCode
);