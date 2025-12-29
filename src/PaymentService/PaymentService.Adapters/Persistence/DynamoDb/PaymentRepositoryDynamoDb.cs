using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using PaymentService.Core.Domain.Entities;
using PaymentService.Core.Domain.Enums;
using PaymentService.Core.Ports.Out;

namespace PaymentService.Adapters.Persistence.DynamoDb;

public sealed class PaymentRepositoryDynamoDb : IPaymentRepository
{
    private readonly IAmazonDynamoDB _ddb;
    private readonly string _table;

    public PaymentRepositoryDynamoDb(IAmazonDynamoDB ddb, IConfiguration cfg)
    {
        _ddb = ddb;
        _table = cfg["DYNAMODB:PAYMENTS_TABLE"] ?? "Payments";
    }

    private static string Pk(string paymentId) => $"PAY#{paymentId}";
    private static string ReservationLookupPk(string reservationId) => $"RESLOOKUP#{reservationId}";

    public async Task CreateAsync(Payment p, CancellationToken ct)
    {
        var paymentItem = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(Pk(p.PaymentId)),
            ["PaymentId"] = new AttributeValue(p.PaymentId),
            ["ReservationId"] = new AttributeValue(p.ReservationId),
            ["CustomerId"] = new AttributeValue(p.CustomerId),
            ["AmountCents"] = new AttributeValue{ N = p.AmountCents.ToString() },
            ["Status"] = new AttributeValue{ N = ((int)p.Status).ToString() },
            ["PaymentCode"] = new AttributeValue(p.PaymentCode),
            ["CreatedAtUtc"] = new AttributeValue(p.CreatedAtUtc.ToString("O")),
            ["PaidAtUtc"] = new AttributeValue(p.PaidAtUtc?.ToString("O") ?? ""),
            ["EntityType"] = new AttributeValue("PAYMENT"),
        };

        var lookupItem = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(ReservationLookupPk(p.ReservationId)),
            ["ReservationId"] = new AttributeValue(p.ReservationId),
            ["PaymentId"] = new AttributeValue(p.PaymentId),
            ["EntityType"] = new AttributeValue("RES_LOOKUP"),
        };

        await _ddb.TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
            {
                new() { Put = new Put { TableName = _table, Item = paymentItem, ConditionExpression = "attribute_not_exists(PK)" } },
                new() { Put = new Put { TableName = _table, Item = lookupItem, ConditionExpression = "attribute_not_exists(PK)" } },
            }
        }, ct);
    }

    public async Task<Payment?> GetByIdAsync(string paymentId, CancellationToken ct)
    {
        var res = await _ddb.GetItemAsync(_table,
            new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(paymentId)) }, ct);

        if (res.Item is null || res.Item.Count == 0) return null;
        return FromItem(res.Item);
    }

    public async Task<Payment?> GetByReservationIdAsync(string reservationId, CancellationToken ct)
    {
        var lookup = await _ddb.GetItemAsync(_table,
            new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(ReservationLookupPk(reservationId)) }, ct);

        if (lookup.Item is null || lookup.Item.Count == 0) return null;

        var paymentId = lookup.Item["PaymentId"].S;
        return await GetByIdAsync(paymentId, ct);
    }

    public async Task UpdateAsync(Payment p, CancellationToken ct)
    {
        // simples: replace do item
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(Pk(p.PaymentId)),
            ["PaymentId"] = new AttributeValue(p.PaymentId),
            ["ReservationId"] = new AttributeValue(p.ReservationId),
            ["CustomerId"] = new AttributeValue(p.CustomerId),
            ["AmountCents"] = new AttributeValue{ N = p.AmountCents.ToString() },
            ["Status"] = new AttributeValue{ N = ((int)p.Status).ToString() },
            ["PaymentCode"] = new AttributeValue(p.PaymentCode),
            ["CreatedAtUtc"] = new AttributeValue(p.CreatedAtUtc.ToString("O")),
            ["PaidAtUtc"] = new AttributeValue(p.PaidAtUtc?.ToString("O") ?? ""),
            ["EntityType"] = new AttributeValue("PAYMENT"),
        };

        await _ddb.PutItemAsync(new PutItemRequest
        {
            TableName = _table,
            Item = item
        }, ct);
    }

    private static Payment FromItem(Dictionary<string, AttributeValue> item)
    {
        DateTime.TryParse(item["CreatedAtUtc"].S, out var createdAt);
        DateTime? paidAt = null;
        if (item.TryGetValue("PaidAtUtc", out var p) && !string.IsNullOrWhiteSpace(p.S))
        {
            if (DateTime.TryParse(p.S, out var dt)) paidAt = dt;
        }

        var statusInt = int.Parse(item["Status"].N);
        var status = (PaymentStatus)statusInt;

        return Payment.Rehydrate(
            item["PaymentId"].S,
            item["ReservationId"].S,
            item["CustomerId"].S,
            long.Parse(item["AmountCents"].N),
            status,
            item["PaymentCode"].S,
            createdAt == default ? DateTime.UtcNow : createdAt,
            paidAt
        );
    }
}