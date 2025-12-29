using PaymentService.Core.Domain.Entities;

namespace PaymentService.Core.Ports.In;

public interface IGetPaymentUseCase
{
    Task<Payment?> ExecuteAsync(string paymentId, CancellationToken ct);
}