using InventoryService.Core.Domain.Enums;
using InventoryService.Core.Domain.Errors;

namespace InventoryService.Core.Domain.Entities;

public sealed class Vehicle
{
    public string VehicleId { get; private set; }
    public string Brand { get; private set; }
    public string Model { get; private set; }
    public int Year { get; private set; }
    public string Color { get; private set; }
    public long PriceCents { get; private set; }
    public VehicleStatus Status { get; private set; }

    public string? ReservationId { get; private set; }
    public string? ReservedByCustomerId { get; private set; }
    public DateTime? ReservationExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set;}
    public DateTime UpdatedAtUtc { get; private set; }

    private Vehicle() { }

    public static Vehicle Create(string brand, string model, int year, string color, long priceCents)
    {
        Validate(brand, model, year, color, priceCents);

        var now = DateTime.UtcNow;
        return new Vehicle
        {
            VehicleId = Guid.NewGuid().ToString("N"),
            Brand = brand.Trim(),
            Model = model.Trim(),
            Year = year,
            Color = color.Trim(),
            PriceCents = priceCents,
            Status = VehicleStatus.Available,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    public void Update(string brand, string model, int year, string color, long priceCents)
    {
        Brand = brand.Trim();
        Model = model.Trim();
        Year = year;
        Color = color.Trim();
        PriceCents = priceCents;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkReserved(string reservationId, string customerId, DateTime expiresAtUtc)
    {
        Status = VehicleStatus.Reserved;
        ReservationId = reservationId;
        ReservedByCustomerId = customerId;
        ReservationExpiresAtUtc = expiresAtUtc;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkAvailable()
    {
        Status = VehicleStatus.Available;
        ReservationId = null;
        ReservedByCustomerId = null;
        ReservationExpiresAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkSold()
    {
        Status = VehicleStatus.Sold;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsReservationExpired(DateTime utcNow)
        => Status == VehicleStatus.Reserved
           && ReservationExpiresAtUtc.HasValue
           && ReservationExpiresAtUtc.Value <= utcNow;

    public static Vehicle Rehydrate(
        string vehicleId,
        string brand,
        string model,
        int year,
        string color,
        long priceCents,
        VehicleStatus status,
        string? reservationId,
        string? reservedByCustomerId,
        DateTime? reservationExpiresAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Validate(brand, model, year, color, priceCents);

        return new Vehicle
        {
            VehicleId = vehicleId,
            Brand = brand.Trim(),
            Model = model.Trim(),
            Year = year,
            Color = color.Trim(),
            PriceCents = priceCents,
            Status = status,
            ReservationId = reservationId,
            ReservedByCustomerId = reservedByCustomerId,
            ReservationExpiresAtUtc = reservationExpiresAtUtc,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }

    private static void Validate(string brand, string model, int year, string color, long priceCents)
    {
        if (string.IsNullOrWhiteSpace(brand)) throw new DomainException("Brand é obrigatório.");
        if (string.IsNullOrWhiteSpace(model)) throw new DomainException("Model é obrigatório.");
        if (string.IsNullOrWhiteSpace(color)) throw new DomainException("Color é obrigatório.");
        if (year < 1900 || year > DateTime.UtcNow.Year + 1) throw new DomainException("Year inválido.");
        if (priceCents <= 0) throw new DomainException("PriceCents deve ser > 0.");
    }
}