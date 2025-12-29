namespace Orchestrator.Api.Contracts.Responses;

public sealed record StartPurchaseResponse(
    string VehicleId,
    string CustomerId,
    string ReservationId,
    string PaymentId,
    string PaymentCode,
    string Status
);