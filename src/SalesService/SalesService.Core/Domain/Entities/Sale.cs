using SalesService.Core.Domain.Errors;

namespace SalesService.Core.Domain.Entities;

public sealed class Sale
{
    public string SaleId { get; private set; } = default!;
    public string VehicleId { get; private set; } = default!;
    public string CustomerId { get; private set; } = default!;
    public string ReservationId { get; private set; } = default!;
    public long PriceCents { get; private set; }

    public DateTime SoldAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Sale() { }

    public static Sale Create(string vehicleId, string customerId, string reservationId, long priceCents)
    {
        if (string.IsNullOrWhiteSpace(vehicleId)) throw new DomainException("VehicleId é obrigatório.");
        if (string.IsNullOrWhiteSpace(customerId)) throw new DomainException("CustomerId é obrigatório.");
        if (string.IsNullOrWhiteSpace(reservationId)) throw new DomainException("ReservationId é obrigatório.");
        if (priceCents <= 0) throw new DomainException("PriceCents deve ser > 0.");

        var now = DateTime.UtcNow;

        return new Sale
        {
            SaleId = Guid.NewGuid().ToString("N"),
            VehicleId = vehicleId,
            CustomerId = customerId,
            ReservationId = reservationId,
            PriceCents = priceCents,
            SoldAtUtc = now,
            CreatedAtUtc = now
        };
    }

    public static Sale Rehydrate(
        string saleId,
        string vehicleId,
        string customerId,
        string reservationId,
        long priceCents,
        DateTime soldAtUtc,
        DateTime createdAtUtc)
    {
        return new Sale
        {
            SaleId = saleId,
            VehicleId = vehicleId,
            CustomerId = customerId,
            ReservationId = reservationId,
            PriceCents = priceCents,
            SoldAtUtc = soldAtUtc,
            CreatedAtUtc = createdAtUtc
        };
    }
}