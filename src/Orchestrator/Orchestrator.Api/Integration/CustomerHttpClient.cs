using System.Net.Http.Json;
using Orchestrator.Core.Ports.Out;

namespace Orchestrator.Adapters.Integration;

public sealed class CustomerHttpClient : ICustomerClient
{
    private readonly HttpClient _http;

    public CustomerHttpClient(HttpClient http) => _http = http;

    public async Task<bool> ExistsAsync(string customerId, CancellationToken ct)
    {
        var res = await _http.GetAsync($"/customers/{customerId}/exists", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<ExistsResponse>(cancellationToken: ct);
        return json?.Exists ?? false;
    }

    private sealed record ExistsResponse(bool Exists);
}