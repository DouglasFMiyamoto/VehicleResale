namespace PaymentService.Api.Contracts.Requests;

public sealed record CreatePaymentRequest(string ReservationId, string CustomerId, long AmountCents);