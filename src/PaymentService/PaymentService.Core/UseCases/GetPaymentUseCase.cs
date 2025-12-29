using PaymentService.Core.Domain.Entities;
using PaymentService.Core.Ports.In;
using PaymentService.Core.Ports.Out;

namespace PaymentService.Core.UseCases;

public sealed class GetPaymentUseCase : IGetPaymentUseCase
{
    private readonly IPaymentRepository _repo;
    public GetPaymentUseCase(IPaymentRepository repo) => _repo = repo;

    public Task<Payment?> ExecuteAsync(string paymentId, CancellationToken ct)
        => _repo.GetByIdAsync(paymentId, ct);
}