using FastEndpoints;
using Microsoft.AspNetCore.Http;
using InventoryService.Api.Contracts.Requests;
using InventoryService.Api.Contracts.Responses;
using InventoryService.Core.Domain.Errors;
using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Endpoints.Vehicles;

public sealed class CreateVehicleEndpoint : Endpoint<CreateVehicleRequest>
{
    private readonly ICreateVehicleUseCase _uc;
    public CreateVehicleEndpoint(ICreateVehicleUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/vehicles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateVehicleRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _uc.ExecuteAsync(
                new CreateVehicleInput(req.Brand, req.Model, req.Year, req.Color, req.PriceCents),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(new CreateVehicleResponse(id), ct);
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