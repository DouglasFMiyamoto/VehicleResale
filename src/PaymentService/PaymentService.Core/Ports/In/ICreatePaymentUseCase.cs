namespace PaymentService.Core.Ports.In;

public interface ICreatePaymentUseCase
{
    Task<CreatePaymentResult> ExecuteAsync(CreatePaymentCommand cmd, CancellationToken ct);
}

public sealed record CreatePaymentCommand(string ReservationId, string CustomerId, long AmountCents);

public sealed record CreatePaymentResult(
    string PaymentId,
    string PaymentCode,
    bool AlreadyExisted
);