using PaymentService.Core.Domain.Entities;

namespace PaymentService.Core.Ports.Out;

public interface IPaymentRepository
{
    Task CreateAsync(Payment payment, CancellationToken ct);
    Task<Payment?> GetByIdAsync(string paymentId, CancellationToken ct);

    // idempotência pra SAGA: 1 pagamento por reservation
    Task<Payment?> GetByReservationIdAsync(string reservationId, CancellationToken ct);

    Task UpdateAsync(Payment payment, CancellationToken ct);
}