using System.Net;
using System.Net.Http.Json;
using Orchestrator.Core.Ports.Out;

namespace Orchestrator.Adapters.Integration;

public sealed class SalesHttpClient : ISalesClient
{
    private readonly HttpClient _http;

    public SalesHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CreateSaleResult> CreateAsync(
        string vehicleId,
        string customerId,
        string reservationId,
        long priceCents,
        CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync(
            "/sales",
            new { vehicleId, customerId, reservationId, priceCents },
            ct);

        if (res.StatusCode == HttpStatusCode.Conflict)
        {
            // se o SalesService tratar duplicidade por reservationId e responder 409,
            // você pode ler o corpo e retornar "AlreadyExisted=true" ou simplesmente falhar.
            // Vou deixar “amigável”:
            var err = await TryReadError(res, ct);
            throw new InvalidOperationException(err ?? "Venda já registrada ou conflito no SalesService.");
        }

        if (res.StatusCode == HttpStatusCode.BadRequest)
        {
            var err = await TryReadError(res, ct);
            throw new InvalidOperationException(err ?? "Requisição inválida ao SalesService.");
        }

        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<CreateSaleResponse>(cancellationToken: ct);

        return new CreateSaleResult(
            body!.SaleId,
            body.AlreadyExisted
        );
    }

    private static async Task<string?> TryReadError(HttpResponseMessage res, CancellationToken ct)
    {
        try
        {
            var err = await res.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);
            return err?.Error;
        }
        catch { return null; }
    }

    private sealed record CreateSaleResponse(string SaleId, bool AlreadyExisted);
    private sealed record ErrorResponse(string Error);
}