using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SalesService.Api.Contracts.Requests;
using SalesService.Api.Contracts.Responses;
using SalesService.Core.Domain.Errors;
using SalesService.Core.Ports.In;

namespace SalesService.Api.Endpoints.Sales;

public sealed class CreateSaleEndpoint : Endpoint<CreateSaleRequest>
{
    private readonly ICreateSaleUseCase _uc;
    public CreateSaleEndpoint(ICreateSaleUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/sales");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateSaleRequest req, CancellationToken ct)
    {
        try
        {
            var result = await _uc.ExecuteAsync(
                new CreateSaleCommand(req.VehicleId, req.CustomerId, req.ReservationId, req.PriceCents),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(
                new CreateSaleResponse(result.SaleId, result.AlreadyExisted),
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