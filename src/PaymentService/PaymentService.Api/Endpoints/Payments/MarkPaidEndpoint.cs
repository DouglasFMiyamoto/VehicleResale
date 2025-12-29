using FastEndpoints;
using Microsoft.AspNetCore.Http;
using PaymentService.Core.Domain.Errors;
using PaymentService.Core.Ports.In;

namespace PaymentService.Api.Endpoints.Payments;

public sealed class MarkPaidEndpoint : EndpointWithoutRequest
{
    private readonly IMarkPaymentPaidUseCase _uc;
    public MarkPaidEndpoint(IMarkPaymentPaidUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/payments/{paymentId}/pay");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var paymentId = Route<string>("paymentId");
            var ok = await _uc.ExecuteAsync(paymentId, ct);

            if (!ok)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Pagamento não encontrado" }, ct);
                return;
            }

            HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        catch (DomainException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}