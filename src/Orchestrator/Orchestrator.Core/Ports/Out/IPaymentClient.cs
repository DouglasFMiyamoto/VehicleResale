namespace Orchestrator.Core.Ports.Out;

public interface IPaymentClient
{
    Task<CreatePaymentResult> CreateAsync(string reservationId, string customerId, long amountCents, CancellationToken ct);

    Task<PaymentStatusResult?> GetStatusAsync(string paymentId, CancellationToken ct);
}

public sealed record CreatePaymentResult(string PaymentId, string PaymentCode, bool AlreadyExisted);

public sealed record PaymentStatusResult(string PaymentId, string Status);