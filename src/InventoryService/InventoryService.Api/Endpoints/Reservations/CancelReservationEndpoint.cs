using FastEndpoints;
using Microsoft.AspNetCore.Http;
using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Endpoints.Reservations;

public sealed class CancelReservationEndpoint : EndpointWithoutRequest
{
    private readonly ICancelReservationUseCase _uc;
    public CancelReservationEndpoint(ICancelReservationUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/reservations/{reservationId}/cancel");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var reservationId = Route<string>("reservationId");
            var ok = await _uc.ExecuteAsync(reservationId, ct);

            if (!ok)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Reserva não encontrada" }, ct);
                return;
            }

            HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}