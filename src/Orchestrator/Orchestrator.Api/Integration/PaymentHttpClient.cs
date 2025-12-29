using System.Net;
using System.Net.Http.Json;
using Orchestrator.Core.Ports.Out;

namespace Orchestrator.Adapters.Integration;

public sealed class PaymentHttpClient : IPaymentClient
{
    private readonly HttpClient _http;

    public PaymentHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<CreatePaymentResult> CreateAsync(
        string reservationId,
        string customerId,
        long amountCents,
        CancellationToken ct)
    {
        var res = await _http.PostAsJsonAsync(
            "/payments",
            new { reservationId, customerId, amountCents },
            ct);

        if (res.StatusCode == HttpStatusCode.BadRequest)
        {
            var err = await TryReadError(res, ct);
            throw new InvalidOperationException(err ?? "Requisição inválida ao PaymentService.");
        }

        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadFromJsonAsync<CreatePaymentResponse>(cancellationToken: ct);

        return new CreatePaymentResult(
            body!.PaymentId,
            body.PaymentCode,
            body.AlreadyExisted
        );
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


    private static async Task<string?> TryReadError(HttpResponseMessage res, CancellationToken ct)
    {
        try
        {
            var err = await res.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: ct);
            return err?.Error;
        }
        catch { return null; }
    }

    private sealed record CreatePaymentResponse(string PaymentId, string PaymentCode, bool AlreadyExisted);
    private sealed record ErrorResponse(string Error);
    private sealed record PaymentStatusResponse(string PaymentId, string Status);
}