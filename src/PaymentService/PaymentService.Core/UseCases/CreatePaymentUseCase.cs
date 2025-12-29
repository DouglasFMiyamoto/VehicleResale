using PaymentService.Core.Domain.Entities;
using PaymentService.Core.Ports.In;
using PaymentService.Core.Ports.Out;

namespace PaymentService.Core.UseCases;

public sealed class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IPaymentRepository _repo;
    public CreatePaymentUseCase(IPaymentRepository repo) => _repo = repo;

    public async Task<CreatePaymentResult> ExecuteAsync(CreatePaymentCommand cmd, CancellationToken ct)
    {
        var existing = await _repo.GetByReservationIdAsync(cmd.ReservationId, ct);
        if (existing is not null)
            return new CreatePaymentResult(existing.PaymentId, existing.PaymentCode, AlreadyExisted: true);

        var p = Payment.Create(cmd.ReservationId, cmd.CustomerId, cmd.AmountCents);
        await _repo.CreateAsync(p, ct);

        return new CreatePaymentResult(p.PaymentId, p.PaymentCode, AlreadyExisted: false);
    }
}