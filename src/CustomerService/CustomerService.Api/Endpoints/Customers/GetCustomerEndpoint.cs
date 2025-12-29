using CustomerService.Api.Contracts.Responses;
using CustomerService.Core.Ports.In;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace CustomerService.Api.Endpoints.Customers;

public class GetCustomerEndpoint : EndpointWithoutRequest
{
    private readonly IGetCustomerUseCase _uc;
    public GetCustomerEndpoint(IGetCustomerUseCase uc) => _uc = uc;

    static string MaskCpf(string cpf)
    {
        var digits = new string((cpf ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length != 11) return "***********";
        return $"{digits[..3]}.***.***-{digits[^2..]}";
    }

    public override void Configure()
    {
        Get("/customers/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");

        var c = await _uc.ExecuteAsync(id ?? string.Empty, ct);

        HttpContext.Response.ContentType = "application/json";

        if (c is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "Customer not found", customerId = id }, ct);
            await HttpContext.Response.CompleteAsync();
            return;
        }

        HttpContext.Response.StatusCode = StatusCodes.Status200OK;
        await HttpContext.Response.WriteAsJsonAsync(new CustomerResponse(
            c.CustomerId,
            c.FullName,
            MaskCpf(c.Document.Value),
            c.Email.Value,
            c.Phone,
            c.AddressLine1,
            c.City,
            c.State,
            c.PostalCode
        ), ct);

        await HttpContext.Response.CompleteAsync();
    }
}