using FastEndpoints;
using Microsoft.AspNetCore.Http;
using PaymentService.Core.Ports.In;

namespace PaymentService.Api.Endpoints.Payments;

public sealed class GetPaymentStatusEndpoint : EndpointWithoutRequest
{
    private readonly IGetPaymentUseCase _uc;

    public GetPaymentStatusEndpoint(IGetPaymentUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Get("/payments/{paymentId}/status");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var paymentId = Route<string>("paymentId");

        var payment = await _uc.ExecuteAsync(paymentId, ct);
        if (payment is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Pagamento não encontrado" }, ct);
            return;
        }

        // status numérico ou string — escolhe UM padrão e mantém.
        await HttpContext.Response.WriteAsJsonAsync(new
        {
            paymentId = payment.PaymentId,
            status = payment.Status.ToString() // "Pending" / "Paid" etc.
        }, ct);
    }
}