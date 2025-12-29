using FastEndpoints;
using Microsoft.AspNetCore.Http;
using InventoryService.Api.Contracts.Responses;
using InventoryService.Core.Domain.Enums;
using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Endpoints.Vehicles;

public sealed class ListSoldVehiclesEndpoint : EndpointWithoutRequest
{
    private readonly IListVehiclesUseCase _uc;
    public ListSoldVehiclesEndpoint(IListVehiclesUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Get("/vehicles/sold");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var items = await _uc.ExecuteAsync(VehicleStatus.Sold, ct);

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(new ListVehiclesResponse(items.ToList()), ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}