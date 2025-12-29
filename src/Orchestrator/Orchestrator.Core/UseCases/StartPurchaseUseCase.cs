using Orchestrator.Core.Ports.In;
using Orchestrator.Core.Ports.Out;

namespace Orchestrator.Core.UseCases;

public sealed class StartPurchaseUseCase : IStartPurchaseUseCase
{
    private readonly ICustomerClient _customers;
    private readonly IInventoryClient _inventory;
    private readonly IPaymentClient _payments;

    public StartPurchaseUseCase(
        ICustomerClient customers,
        IInventoryClient inventory,
        IPaymentClient payments)
    {
        _customers = customers;
        _inventory = inventory;
        _payments = payments;
    }

    public async Task<StartPurchaseResult> ExecuteAsync(StartPurchaseCommand cmd, CancellationToken ct)
    {
        if (!await _customers.ExistsAsync(cmd.CustomerId, ct))
            throw new InvalidOperationException("Comprador não encontrado.");

        var reserve = await _inventory.ReserveAsync(cmd.VehicleId, cmd.CustomerId, ct);
        if (!reserve.Success || reserve.ReservationId is null)
            throw new InvalidOperationException(reserve.Error ?? "Falha ao reservar veículo.");

        try
        {
            var payment = await _payments.CreateAsync(
                reserve.ReservationId,
                cmd.CustomerId,
                cmd.PriceCents,
                ct);

            return new StartPurchaseResult(
                cmd.VehicleId,
                cmd.CustomerId,
                reserve.ReservationId,
                payment.PaymentId,
                payment.PaymentCode);
        }
        catch
        {
            await _inventory.CancelReservationAsync(reserve.ReservationId, ct);
            throw;
        }
    }
}