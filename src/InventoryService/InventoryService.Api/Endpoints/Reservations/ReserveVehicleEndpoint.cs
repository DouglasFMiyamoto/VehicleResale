using FastEndpoints;
using Microsoft.AspNetCore.Http;
using InventoryService.Api.Contracts.Requests;
using InventoryService.Api.Contracts.Responses;
using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Endpoints.Reservations;

public sealed class ReserveVehicleEndpoint : Endpoint<ReserveVehicleRequest>
{
    private readonly IReserveVehicleUseCase _uc;
    public ReserveVehicleEndpoint(IReserveVehicleUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/vehicles/{vehicleId}/reserve");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ReserveVehicleRequest req, CancellationToken ct)
    {
        try
        {
            var vehicleId = Route<string>("vehicleId");
            var result = await _uc.ExecuteAsync(new ReserveVehicleInput(vehicleId, req.CustomerId), ct);

            if (!result.Success)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await HttpContext.Response.WriteAsJsonAsync(
                    new ReserveVehicleResponse(false, null, null, result.Error ?? "Conflito ao reservar"),
                    ct);
                return;
            }

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(
                new ReserveVehicleResponse(true, result.ReservationId, result.ExpiresAtUtc, null),
                ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}