using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Orchestrator.Api.Contracts.Requests;
using Orchestrator.Api.Contracts.Responses;
using Orchestrator.Core.Ports.In;

namespace Orchestrator.Api.Endpoints.Purchase;

public sealed class StartPurchaseEndpoint : Endpoint<StartPurchaseRequest>
{
    private readonly IStartPurchaseUseCase _uc;
    public StartPurchaseEndpoint(IStartPurchaseUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/orchestrator/purchase/{vehicleId}/start");
        AllowAnonymous();
    }

    public override async Task HandleAsync(StartPurchaseRequest req, CancellationToken ct)
    {
        try
        {
            var vehicleId = Route<string>("vehicleId");

            // PriceCents: por enquanto vem do frontend/orchestrator (POC).
            // Em produção, viria do Inventory (source of truth).
            var priceCents = Query<long>("priceCents", isRequired: true);

            var result = await _uc.ExecuteAsync(
                new StartPurchaseCommand(vehicleId, req.CustomerId, priceCents),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(
                new StartPurchaseResponse(
                    result.VehicleId, result.CustomerId,
                    result.ReservationId, result.PaymentId, result.PaymentCode,
                    "WAITING_PAYMENT"),
                ct);
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