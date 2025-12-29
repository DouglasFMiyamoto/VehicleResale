using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Ports.Out;
using Microsoft.Extensions.Configuration;

namespace InventoryService.Adapters.Persistence.DynamoDb;

public sealed class ReservationRepositoryDynamoDb : IReservationRepository
{
    private readonly IAmazonDynamoDB _ddb;
    private readonly string _table;

    public ReservationRepositoryDynamoDb(IAmazonDynamoDB ddb, IConfiguration cfg)
    {
        _ddb = ddb;
        _table = cfg["DYNAMODB:RESERVATIONS_TABLE"] ?? "Reservations";
    }

    private static string Pk(string id) => $"RES#{id}";

    public async Task CreateAsync(Reservation r, CancellationToken ct)
    {
        await _ddb.PutItemAsync(new PutItemRequest
        {
            TableName = _table,
            Item = ToItem(r)
        }, ct);
    }

    public async Task<Reservation?> GetByIdAsync(string reservationId, CancellationToken ct)
    {
        var res = await _ddb.GetItemAsync(_table,
            new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(reservationId)) }, ct);

        if (res.Item is null || res.Item.Count == 0) return null;
        return FromItem(res.Item);
    }

    public async Task UpdateAsync(Reservation r, CancellationToken ct)
    {
        await _ddb.PutItemAsync(new PutItemRequest
        {
            TableName = _table,
            Item = ToItem(r)
        }, ct);
    }

    private static Dictionary<string, AttributeValue> ToItem(Reservation r)
    {
        return new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(Pk(r.ReservationId)),
            ["ReservationId"] = new AttributeValue(r.ReservationId),
            ["VehicleId"] = new AttributeValue(r.VehicleId),
            ["CustomerId"] = new AttributeValue(r.CustomerId),
            ["Status"] = new AttributeValue(r.Status.ToString()),
            ["ExpiresAtUtc"] = new AttributeValue(r.ExpiresAtUtc.ToString("O")),
            ["CreatedAtUtc"] = new AttributeValue(r.CreatedAtUtc.ToString("O")),
            ["UpdatedAtUtc"] = new AttributeValue(r.UpdatedAtUtc.ToString("O")),
        };
    }

    private static Reservation FromItem(Dictionary<string, AttributeValue> item)
    {
        DateTime.TryParse(item["ExpiresAtUtc"].S, out var exp);
        DateTime.TryParse(item["CreatedAtUtc"].S, out var created);
        DateTime.TryParse(item["UpdatedAtUtc"].S, out var updated);

        var status = Enum.Parse<ReservationStatus>(item["Status"].S);

        return Reservation.Rehydrate(
            item["ReservationId"].S,
            item["VehicleId"].S,
            item["CustomerId"].S,
            status,
            exp,
            created == default ? DateTime.UtcNow : created,
            updated == default ? DateTime.UtcNow : updated
        );
    }
}