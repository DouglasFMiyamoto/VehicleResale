using CustomerService.Api.Contracts.Responses;
using CustomerService.Core.Ports.In;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace CustomerService.Api.Endpoints.Customers;

public class ExistsCustomerEndpoint : EndpointWithoutRequest
{
    private readonly IExistsCustomerUseCase _uc;
    public ExistsCustomerEndpoint(IExistsCustomerUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Get("/customers/{id}/exists");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var exists = await _uc.ExecuteAsync(id, ct);

        HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        await HttpContext.Response.WriteAsJsonAsync(
            new ExistsCustomerResponse(exists), ct);
    }
}
