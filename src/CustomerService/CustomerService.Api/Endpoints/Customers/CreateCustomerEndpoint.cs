using CustomerService.Api.Contracts.Requests;
using CustomerService.Api.Contracts.Responses;
using CustomerService.Core.Domain.Errors;
using CustomerService.Core.Ports.In;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace CustomerService.Api.Endpoints.Customers;

public class CreateCustomerEndpoint : Endpoint<CreateCustomerRequest>
{
    private readonly ICreateCustomerUseCase _uc;
    public CreateCustomerEndpoint(ICreateCustomerUseCase uc) => _uc = uc;

    public override void Configure()
    {
        Post("/customers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        try
        {
            var created = await _uc.ExecuteAsync(
                new CreateCustomerCommand(
                    req.FullName, req.DocumentCpf, req.Email, req.Phone,
                    req.AddressLine1, req.City, req.State, req.PostalCode),
                ct);

            HttpContext.Response.StatusCode = StatusCodes.Status201Created;
            await HttpContext.Response.WriteAsJsonAsync(
                new CreateCustomerResponse(created.CustomerId), ct);
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
