using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Orchestrator.Api.Contracts.Requests;
using Orchestrator.Api.Contracts.Responses;
using Orchestrator.Core.Ports.In;

namespace Orchestrator.Api.Endpoints.Purchase;

public sealed class FinalizePurchaseEndpoint : Endpoint<FinalizePurchaseRequest>
{
    private readonly IFinalizePurchaseUseCase _uc;
    public FinalizePurchaseEndpoint(IFinalizePurchaseUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/orchestrator/purchase/{vehicleId}/finalize");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FinalizePurchaseRequest req, CancellationToken ct)
    {
        try
        {
            var vehicleId = Route<string>("vehicleId");

            var result = await _uc.ExecuteAsync(
                new FinalizePurchaseCommand(vehicleId, req.CustomerId, req.ReservationId, req.PaymentId, req.PriceCents),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(new FinalizePurchaseResponse(result.SaleId, result.Status), ct);
        }
        catch (InvalidOperationException ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await HttpContext.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}