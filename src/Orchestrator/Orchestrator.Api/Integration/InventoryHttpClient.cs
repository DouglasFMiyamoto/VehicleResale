using System.Net;
using System.Net.Http.Json;
using Orchestrator.Core.Ports.Out;

namespace Orchestrator.Adapters.Integration;

public sealed class InventoryHttpClient : IInventoryClient
{
    private readonly HttpClient _http;

    public InventoryHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ReserveResult> ReserveAsync(
        string vehicleId,
        string customerId,
        CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync(
            $"/vehicles/{vehicleId}/reserve",
            new { customerId },
            ct);

        if (res.StatusCode == HttpStatusCode.Conflict)
        {
            var err = await res.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);
            return new ReserveResult(false, null, null, err?.Error ?? "Veículo indisponível");
        }

        if (res.StatusCode == HttpStatusCode.NotFound)
        {
            return new ReserveResult(false, null, null, "Veículo não encontrado");
        }

        res.EnsureSuccessStatusCode();

        var ok = await res.Content.ReadFromJsonAsync<ReserveResponse>(cancellationToken: ct);

        return new ReserveResult(
            true,
            ok!.ReservationId,
            ok.ExpiresAtUtc,
            null
        );
    }

    public async Task CancelReservationAsync(string reservationId, CancellationToken ct)
    {
        var res = await _http.PostAsync(
            $"/reservations/{reservationId}/cancel",
            content: null,
            ct);

        // Compensação → best effort
        if (res.StatusCode == HttpStatusCode.NotFound ||
            res.StatusCode == HttpStatusCode.Conflict)
            return;

        res.EnsureSuccessStatusCode();
    }

    public async Task<bool> ConfirmSaleAsync(string reservationId, CancellationToken ct)
    {
        var res = await _http.PostAsync(
            $"/reservations/{reservationId}/confirm-sale",
            content: null,
            ct);

        if (res.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (res.StatusCode == HttpStatusCode.Conflict)
            return false;

        res.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<PaymentStatusResult?> GetStatusAsync(string paymentId, CancellationToken ct)
    {
        var res = await _http.GetAsync($"/payments/{paymentId}/status", ct);

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<PaymentStatusResponse>(cancellationToken: ct);
        return new PaymentStatusResult(body!.PaymentId, body.Status);
    } 

    // ===== DTOs internos =====

    private sealed record ReserveResponse(
        bool Success,
        string ReservationId,
        DateTime ExpiresAtUtc
    );

    private sealed record ErrorResponse(string Error);
    private sealed record PaymentStatusResponse(string PaymentId, string Status);
}