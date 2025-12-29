using Orchestrator.Core.Ports.In;
using Orchestrator.Core.Ports.Out;

namespace Orchestrator.Core.UseCases;

public sealed class FinalizePurchaseUseCase : IFinalizePurchaseUseCase
{
    private readonly IInventoryClient _inventory;
    private readonly ISalesClient _sales;
    private readonly IPaymentClient _payments;

    public FinalizePurchaseUseCase(IInventoryClient inventory, ISalesClient sales, IPaymentClient payments)
    {
        _inventory = inventory;
        _sales = sales;
        _payments = payments;
    }

    public async Task<FinalizePurchaseResult> ExecuteAsync(FinalizePurchaseCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.VehicleId))
            throw new InvalidOperationException("VehicleId é obrigatório.");

        if (string.IsNullOrWhiteSpace(cmd.CustomerId))
            throw new InvalidOperationException("CustomerId é obrigatório.");

        if (string.IsNullOrWhiteSpace(cmd.ReservationId))
            throw new InvalidOperationException("ReservationId é obrigatório.");

        var pay = await _payments.GetStatusAsync(cmd.PaymentId, ct);
        if (pay is null)
        {
            await _inventory.CancelReservationAsync(cmd.ReservationId, ct); // opcional
            throw new InvalidOperationException("Pagamento não encontrado.");
        }

        if (!string.Equals(pay.Status, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            // opcional: manter reserva ativa até expirar, ou cancelar agora.
            // para “demo bonita”, eu cancelaria:
            await _inventory.CancelReservationAsync(cmd.ReservationId, ct);
            throw new InvalidOperationException("Pagamento ainda não foi confirmado.");
        }

        // MVP: pagamento já foi marcado como PAID manualmente.
        // Em produção: validar status no PaymentService antes de confirmar a venda.

        // 1) Confirmar venda no Inventory (baixa estoque / marca como SOLD)
        var confirmed = await _inventory.ConfirmSaleAsync(cmd.ReservationId, ct);

        if (!confirmed)
        {
            // tentativa de compensação (best effort)
            await _inventory.CancelReservationAsync(cmd.ReservationId, ct);
            throw new InvalidOperationException("Não foi possível confirmar a venda (reserva inválida, expirada ou já processada).");
        }

        try
        {
            // 2) Registrar venda no Sales (idempotente por reservationId lá)
            var sale = await _sales.CreateAsync(
                cmd.VehicleId,
                cmd.CustomerId,
                cmd.ReservationId,
                cmd.PriceCents,
                ct);

            // Se a SAGA chamar 2x, Sales devolve AlreadyExisted=true mas você pode retornar COMPLETED igual.
            return new FinalizePurchaseResult(sale.SaleId, "COMPLETED");
        }
        catch
        {
            // compensação simples (POC):
            // se falhar registrar venda, cancela reserva pra não "travar" o veículo
            // (em produção, melhor seria "marcar pendente" e ter retry)
            await _inventory.CancelReservationAsync(cmd.ReservationId, ct);
            throw;
        }
    }
}