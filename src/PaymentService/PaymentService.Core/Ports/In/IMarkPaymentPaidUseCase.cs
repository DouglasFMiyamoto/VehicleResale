namespace PaymentService.Core.Ports.In;

public interface IMarkPaymentPaidUseCase
{
    Task<bool> ExecuteAsync(string paymentId, CancellationToken ct);
}