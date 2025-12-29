namespace PaymentService.Api.Contracts.Responses;

public sealed record CreatePaymentResponse(string PaymentId, string PaymentCode, bool AlreadyExisted);