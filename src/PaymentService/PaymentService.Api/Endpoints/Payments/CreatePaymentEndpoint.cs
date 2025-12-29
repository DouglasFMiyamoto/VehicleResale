using FastEndpoints;
using Microsoft.AspNetCore.Http;
using PaymentService.Api.Contracts.Requests;
using PaymentService.Api.Contracts.Responses;
using PaymentService.Core.Domain.Errors;
using PaymentService.Core.Ports.In;

namespace PaymentService.Api.Endpoints.Payments;

public sealed class CreatePaymentEndpoint : Endpoint<CreatePaymentRequest>
{
    private readonly ICreatePaymentUseCase _uc;
    public CreatePaymentEndpoint(ICreatePaymentUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/payments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreatePaymentRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _uc.ExecuteAsync(
                new CreatePaymentCommand(req.ReservationId, req.CustomerId, req.AmountCents),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(
                new CreatePaymentResponse(result.PaymentId, result.PaymentCode, result.AlreadyExisted),
                ct);
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