using System.Net.Http.Json;

namespace Orchestrator.Api.Integration;

public sealed class CustomerClient(HttpClient http)
{
    public async Task<bool> ExistsAsync(string customerId, CancellationToken ct)
    {
        var res = await http.GetAsync($"/customers/{customerId}/exists", ct);
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<ExistsCustomerResponse>(cancellationToken: ct);
        return json?.Exists ?? false;
    }

    private sealed record ExistsCustomerResponse(bool Exists);
}

public sealed class InventoryClient(HttpClient http)
{
    public async Task<ReserveVehicleResult> ReserveAsync(string vehicleId, string customerId, CancellationToken ct)
    {
        var res = await http.PostAsJsonAsync($"/vehicles/{vehicleId}/reserve", new { customerId }, ct);
        if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var err = await res.Content.ReadFromJsonAsync<ReserveVehicleResponse>(cancellationToken: ct);
            return new ReserveVehicleResult(false, null, null, err?.Error ?? "Conflito ao reservar");
        }
        res.EnsureSuccessStatusCode();
        var ok = await res.Content.ReadFromJsonAsync<ReserveVehicleResponse>(cancellationToken: ct);
        return new ReserveVehicleResult(true, ok!.ReservationId!, ok.ExpiresAtUtc, null);
    }

    public async Task CancelReservationAsync(string reservationId, CancellationToken ct)
    {
        var res = await http.PostAsync($"/reservations/{reservationId}/cancel", content: null, ct);
        // Cancel é compensação; não vale explodir se já cancelou
        if (!res.IsSuccessStatusCode && res.StatusCode != System.Net.HttpStatusCode.NotFound)
            res.EnsureSuccessStatusCode();
    }

    public async Task<bool> ConfirmSaleAsync(string reservationId, CancellationToken ct)
    {
        var res = await http.PostAsync($"/reservations/{reservationId}/confirm-sale", content: null, ct);
        if (res.StatusCode == System.Net.HttpStatusCode.Conflict) return false;
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return false;
        res.EnsureSuccessStatusCode();
        return true;
    }

    private sealed record ReserveVehicleResponse(bool Success, string? ReservationId, DateTime? ExpiresAtUtc, string? Error);

    public sealed record ReserveVehicleResult(bool Success, string? ReservationId, DateTime? ExpiresAtUtc, string? Error);
}

public sealed class PaymentClient(HttpClient http)
{
    public async Task<CreatePaymentResult> CreateAsync(string reservationId, string customerId, long amountCents, CancellationToken ct)
    {
        var res = await http.PostAsJsonAsync("/payments", new { reservationId, customerId, amountCents }, ct);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken: ct);
        return new CreatePaymentResult(json!.PaymentId, json.PaymentCode, json.AlreadyExisted);
    }

    private sealed record CreatePaymentResponse(string PaymentId, string PaymentCode, bool AlreadyExisted);
    public sealed record CreatePaymentResult(string PaymentId, string PaymentCode, bool AlreadyExisted);
}

public sealed class SalesClient(HttpClient http)
{
    public async Task<CreateSaleResult> CreateAsync(string vehicleId, string customerId, string reservationId, long priceCents, CancellationToken ct)
    {
        var res = await http.PostAsJsonAsync("/sales", new { vehicleId, customerId, reservationId, priceCents }, ct);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<CreateSaleResponse>(cancellationToken: ct);
        return new CreateSaleResult(json!.SaleId, json.AlreadyExisted);
    }

    private sealed record CreateSaleResponse(string SaleId, bool AlreadyExisted);
    public sealed record CreateSaleResult(string SaleId, bool AlreadyExisted);
}