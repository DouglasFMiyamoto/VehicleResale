using FastEndpoints;
using Microsoft.AspNetCore.Http;
using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Endpoints.Reservations;

public sealed class ConfirmSaleEndpoint : EndpointWithoutRequest
{
    private readonly IConfirmSaleUseCase _uc;
    public ConfirmSaleEndpoint(IConfirmSaleUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/reservations/{reservationId}/confirm-sale");
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
                HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Não foi possível confirmar a venda (reserva inválida/expirada)." }, ct);
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