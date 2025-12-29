using FastEndpoints;
using Microsoft.AspNetCore.Http;
using SalesService.Api.Contracts.Responses;
using SalesService.Core.Ports.In;

namespace SalesService.Api.Endpoints.Sales;

public sealed class ListSalesEndpoint : EndpointWithoutRequest
{
    private readonly IListSalesUseCase _uc;
    public ListSalesEndpoint(IListSalesUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Get("/sales");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var items = await _uc.ExecuteAsync(ct);

            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            await HttpContext.Response.WriteAsJsonAsync(new ListSalesResponse(items.ToList()), ct);
        }
        catch (Exception)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Erro interno" }, ct);
        }
    }
}