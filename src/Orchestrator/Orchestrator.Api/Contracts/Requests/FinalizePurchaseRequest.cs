namespace Orchestrator.Api.Contracts.Requests;

public sealed record FinalizePurchaseRequest(
    string CustomerId,
    string ReservationId,
    string PaymentId,
    long PriceCents
);