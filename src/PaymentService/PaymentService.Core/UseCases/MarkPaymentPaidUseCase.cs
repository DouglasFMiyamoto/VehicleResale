using PaymentService.Core.Domain.Errors;
using PaymentService.Core.Ports.In;
using PaymentService.Core.Ports.Out;

namespace PaymentService.Core.UseCases;

public sealed class MarkPaymentPaidUseCase : IMarkPaymentPaidUseCase
{
    private readonly IPaymentRepository _repo;
    public MarkPaymentPaidUseCase(IPaymentRepository repo) => _repo = repo;

    public async Task<bool> ExecuteAsync(string paymentId, CancellationToken ct)
    {
        var p = await _repo.GetByIdAsync(paymentId, ct);
        if (p is null) return false;

        try
        {
            p.MarkAsPaid();
            await _repo.UpdateAsync(p, ct);
            return true;
        }
        catch (DomainException)
        {
            // não deixa explodir o fluxo; SAGA decide o que fazer
            throw;
        }
    }
}