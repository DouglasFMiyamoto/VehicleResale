using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SalesService.Core.Ports.In;

namespace SalesService.Api.Endpoints.Sales;

public sealed class GetSaleEndpoint : EndpointWithoutRequest
{
    private readonly IGetSaleUseCase _uc;
    public GetSaleEndpoint(IGetSaleUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Get("/sales/{saleId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var saleId = Route<string>("saleId");
            var sale = await _uc.ExecuteAsync(saleId, ct);

            if (sale is null)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await HttpContext.Response.WriteAsJsonAsync(new { error = "Venda não encontrada" }, ct);
                return;
            }

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(sale, ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}