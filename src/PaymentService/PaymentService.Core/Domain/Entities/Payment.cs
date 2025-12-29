using PaymentService.Core.Domain.Enums;
using PaymentService.Core.Domain.Errors;

namespace PaymentService.Core.Domain.Entities;

public sealed class Payment
{
    public string PaymentId { get; private set; } = default!;
    public string ReservationId { get; private set; } = default!;
    public string CustomerId { get; private set; } = default!;
    public long AmountCents { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string PaymentCode { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? PaidAtUtc { get; private set; }

    private Payment() { }

    public static Payment Create(string reservationId, string customerId, long amountCents)
    {
        if (string.IsNullOrWhiteSpace(reservationId)) throw new DomainException("ReservationId é obrigatório.");
        if (string.IsNullOrWhiteSpace(customerId)) throw new DomainException("CustomerId é obrigatório.");
        if (amountCents <= 0) throw new DomainException("AmountCents deve ser > 0.");

        var now = DateTime.UtcNow;

        return new Payment
        {
            PaymentId = Guid.NewGuid().ToString("N"),
            ReservationId = reservationId,
            CustomerId = customerId,
            AmountCents = amountCents,
            Status = PaymentStatus.Pending,
            PaymentCode = $"PAY-{Guid.NewGuid():N}".Substring(0, 12).ToUpperInvariant(),
            CreatedAtUtc = now
        };
    }

    public void MarkAsPaid()
    {
        if (Status == PaymentStatus.Paid) return;
        if (Status == PaymentStatus.Cancelled) throw new DomainException("Pagamento cancelado não pode ser pago.");
        if (Status == PaymentStatus.Failed) throw new DomainException("Pagamento com falha não pode ser pago.");

        Status = PaymentStatus.Paid;
        PaidAtUtc = DateTime.UtcNow;
    }

    public static Payment Rehydrate(
        string paymentId,
        string reservationId,
        string customerId,
        long amountCents,
        PaymentStatus status,
        string paymentCode,
        DateTime createdAtUtc,
        DateTime? paidAtUtc)
    {
        return new Payment
        {
            PaymentId = paymentId,
            ReservationId = reservationId,
            CustomerId = customerId,
            AmountCents = amountCents,
            Status = status,
            PaymentCode = paymentCode,
            CreatedAtUtc = createdAtUtc,
            PaidAtUtc = paidAtUtc
        };
    }
}