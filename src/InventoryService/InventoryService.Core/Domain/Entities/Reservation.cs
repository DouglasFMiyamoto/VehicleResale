using InventoryService.Core.Domain.Errors;

namespace InventoryService.Core.Domain.Entities;

public enum ReservationStatus
{
    Active = 1,
    Cancelled = 2,
    Completed = 3,
    Expired = 4
}

public sealed class Reservation
{
    public string ReservationId { get; private set; } = default!;
    public string VehicleId { get; private set; } = default!;
    public string CustomerId { get; private set; } = default!;
    public ReservationStatus Status { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Reservation() { }

    public static Reservation Create(string vehicleId, string customerId, int ttlMinutes)
    {
        if (string.IsNullOrWhiteSpace(vehicleId)) throw new DomainException("VehicleId é obrigatório.");
        if (string.IsNullOrWhiteSpace(customerId)) throw new DomainException("CustomerId é obrigatório.");
        if (ttlMinutes < 1 || ttlMinutes > 24 * 60) throw new DomainException("TTL inválido.");

        var now = DateTime.UtcNow;
        return new Reservation
        {
            ReservationId = Guid.NewGuid().ToString("N"),
            VehicleId = vehicleId,
            CustomerId = customerId,
            Status = ReservationStatus.Active,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(ttlMinutes)
        };
    }

    public void MarkCancelled()
    {
        Status = ReservationStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Status = ReservationStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MarkExpired()
    {
        Status = ReservationStatus.Expired;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Reservation Rehydrate(
        string reservationId,
        string vehicleId,
        string customerId,
        ReservationStatus status,
        DateTime expiresAtUtc,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        return new Reservation
        {
            ReservationId = reservationId,
            VehicleId = vehicleId,
            CustomerId = customerId,
            Status = status,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = updatedAtUtc
        };
    }
}