using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using InventoryService.Core.Domain.Entities;
using InventoryService.Core.Domain.Enums;
using InventoryService.Core.Ports.Out;
using Microsoft.Extensions.Configuration;

namespace InventoryService.Adapters.Persistence.DynamoDb;

public sealed class VehicleRepositoryDynamoDb : IVehicleRepository
{
    private readonly IAmazonDynamoDB _ddb;
    private readonly string _table;

    public VehicleRepositoryDynamoDb(IAmazonDynamoDB ddb, IConfiguration cfg)
    {
        _ddb = ddb;
        _table = cfg["DYNAMODB:VEHICLES_TABLE"] ?? "Vehicles";
    }

    private static string Pk(string id) => $"VEH#{id}";

    public async Task CreateAsync(Vehicle v, CancellationToken ct)
    {
        await _ddb.PutItemAsync(new PutItemRequest
        {
            TableName = _table,
            Item = ToItem(v)
        }, ct);
    }

    public async Task<Vehicle?> GetByIdAsync(string vehicleId, CancellationToken ct)
    {
        var res = await _ddb.GetItemAsync(_table,
            new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(vehicleId)) }, ct);

        if (res.Item is null || res.Item.Count == 0) return null;
        return FromItem(res.Item);
    }

    public async Task UpdateAsync(Vehicle v, CancellationToken ct)
    {
        await _ddb.PutItemAsync(new PutItemRequest
        {
            TableName = _table,
            Item = ToItem(v)
        }, ct);
    }

    public async Task<IReadOnlyList<Vehicle>> ListByStatusAsync(VehicleStatus status, CancellationToken ct)
    {
        // POC: Scan + filter; produção: GSI Status + Price
        var scan = await _ddb.ScanAsync(new ScanRequest
        {
            TableName = _table,
            FilterExpression = "#s = :st",
            ExpressionAttributeNames = new Dictionary<string, string> { ["#s"] = "Status" },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":st"] = new AttributeValue(status.ToString())
            }
        }, ct);

        var list = new List<Vehicle>();
        foreach (var item in scan.Items)
            list.Add(FromItem(item));

        return list;
    }

    public async Task<bool> TryReserveAsync(string vehicleId, string reservationId, string customerId, DateTime expiresAtUtc, CancellationToken ct)
    {
        try
        {
            await _ddb.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = _table,
                Key = new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(vehicleId)) },
                ConditionExpression = "#s = :available",
                UpdateExpression = "SET #s = :reserved, ReservationId = :rid, ReservedByCustomerId = :cid, ReservationExpiresAtUtc = :exp, UpdatedAtUtc = :u",
                ExpressionAttributeNames = new Dictionary<string, string> { ["#s"] = "Status" },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":available"] = new AttributeValue(VehicleStatus.Available.ToString()),
                    [":reserved"] = new AttributeValue(VehicleStatus.Reserved.ToString()),
                    [":rid"] = new AttributeValue(reservationId),
                    [":cid"] = new AttributeValue(customerId),
                    [":exp"] = new AttributeValue(expiresAtUtc.ToString("O")),
                    [":u"] = new AttributeValue(DateTime.UtcNow.ToString("O")),
                }
            }, ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    public async Task<bool> TryReleaseReservationAsync(string vehicleId, string reservationId, CancellationToken ct)
    {
        try
        {
            await _ddb.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = _table,
                Key = new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(vehicleId)) },
                ConditionExpression = "#s = :reserved AND ReservationId = :rid",
                UpdateExpression = "SET #s = :available REMOVE ReservationId, ReservedByCustomerId, ReservationExpiresAtUtc",
                ExpressionAttributeNames = new Dictionary<string, string> { ["#s"] = "Status" },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":reserved"] = new AttributeValue(VehicleStatus.Reserved.ToString()),
                    [":available"] = new AttributeValue(VehicleStatus.Available.ToString()),
                    [":rid"] = new AttributeValue(reservationId),
                }
            }, ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    public async Task<bool> TryMarkSoldAsync(string vehicleId, string reservationId, CancellationToken ct)
    {
        try
        {
            await _ddb.UpdateItemAsync(new UpdateItemRequest
            {
                TableName = _table,
                Key = new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(vehicleId)) },
                ConditionExpression = "#s = :reserved AND ReservationId = :rid",
                UpdateExpression = "SET #s = :sold, UpdatedAtUtc = :u",
                ExpressionAttributeNames = new Dictionary<string, string> { ["#s"] = "Status" },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":reserved"] = new AttributeValue(VehicleStatus.Reserved.ToString()),
                    [":sold"] = new AttributeValue(VehicleStatus.Sold.ToString()),
                    [":rid"] = new AttributeValue(reservationId),
                    [":u"] = new AttributeValue(DateTime.UtcNow.ToString("O")),
                }
            }, ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
    }

    private static Dictionary<string, AttributeValue> ToItem(Vehicle v)
    {
        return new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(Pk(v.VehicleId)),
            ["VehicleId"] = new AttributeValue(v.VehicleId),
            ["Brand"] = new AttributeValue(v.Brand),
            ["Model"] = new AttributeValue(v.Model),
            ["Year"] = new AttributeValue { N = v.Year.ToString() },
            ["Color"] = new AttributeValue(v.Color),
            ["PriceCents"] = new AttributeValue { N = v.PriceCents.ToString() },
            ["Status"] = new AttributeValue(v.Status.ToString()),
            ["ReservationId"] = v.ReservationId is null ? new AttributeValue { NULL = true } : new AttributeValue(v.ReservationId),
            ["ReservedByCustomerId"] = v.ReservedByCustomerId is null ? new AttributeValue { NULL = true } : new AttributeValue(v.ReservedByCustomerId),
            ["ReservationExpiresAtUtc"] = v.ReservationExpiresAtUtc is null ? new AttributeValue { NULL = true } : new AttributeValue(v.ReservationExpiresAtUtc.Value.ToString("O")),
            ["CreatedAtUtc"] = new AttributeValue(v.CreatedAtUtc.ToString("O")),
            ["UpdatedAtUtc"] = new AttributeValue(v.UpdatedAtUtc.ToString("O")),
        };
    }

    private static Vehicle FromItem(Dictionary<string, AttributeValue> item)
    {
        DateTime.TryParse(item["CreatedAtUtc"].S, out var created);
        DateTime.TryParse(item["UpdatedAtUtc"].S, out var updated);

        DateTime? exp = null;
        if (item.TryGetValue("ReservationExpiresAtUtc", out var expAttr) && expAttr.NULL != true && !string.IsNullOrWhiteSpace(expAttr.S))
        {
            if (DateTime.TryParse(expAttr.S, out var parsed)) exp = parsed;
        }

        var status = Enum.Parse<VehicleStatus>(item["Status"].S);

        string? rid = null;
        if (item.TryGetValue("ReservationId", out var ridAttr) && ridAttr.NULL != true) rid = ridAttr.S;

        string? cid = null;
        if (item.TryGetValue("ReservedByCustomerId", out var cidAttr) && cidAttr.NULL != true) cid = cidAttr.S;

        return Vehicle.Rehydrate(
            item["VehicleId"].S,
            item["Brand"].S,
            item["Model"].S,
            int.Parse(item["Year"].N),
            item["Color"].S,
            long.Parse(item["PriceCents"].N),
            status,
            rid,
            cid,
            exp,
            created == default ? DateTime.UtcNow : created,
            updated == default ? DateTime.UtcNow : updated
        );
    }
}