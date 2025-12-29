using FastEndpoints;
using Microsoft.AspNetCore.Http;
using InventoryService.Api.Contracts.Requests;
using InventoryService.Core.Domain.Errors;
using InventoryService.Core.Ports.In;

namespace InventoryService.Api.Endpoints.Vehicles;

public sealed class UpdateVehicleEndpoint : Endpoint<UpdateVehicleRequest>
{
    private readonly IUpdateVehicleUseCase _uc;
    public UpdateVehicleEndpoint(IUpdateVehicleUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Put("/vehicles/{vehicleId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateVehicleRequest req, CancellationToken ct)
    {
        try
        {
            var vehicleId = Route<string>("vehicleId");

            await _uc.ExecuteAsync(
                new UpdateVehicleInput(vehicleId, req.Brand, req.Model, req.Year, req.Color, req.PriceCents),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
        }
        catch (DomainException ex)
        {
            // aqui você pode decidir: se for "não encontrado" -> 404, se for validação -> 400.
            // Para manter simples: se mensagem contiver "não encontrado" => 404
            if (ex.Message.Contains("não encontrado", StringComparison.OrdinalIgnoreCase))
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            else
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