using FastEndpoints;
using Microsoft.AspNetCore.Http;
using PaymentService.Core.Ports.In;

namespace PaymentService.Api.Endpoints.Payments;

public sealed class GetPaymentEndpoint : EndpointWithoutRequest
{
    private readonly IGetPaymentUseCase _uc;
    public GetPaymentEndpoint(IGetPaymentUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Get("/payments/{paymentId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var paymentId = Route<string>("paymentId");
            var p = await _uc.ExecuteAsync(paymentId, ct);

            if (p is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Pagamento não encontrado" }, ct);
                return;
            }

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(p, ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}