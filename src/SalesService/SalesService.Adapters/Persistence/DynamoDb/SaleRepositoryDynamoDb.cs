using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using SalesService.Core.Domain.Entities;
using SalesService.Core.Ports.Out;

namespace SalesService.Adapters.Persistence.DynamoDb;

public sealed class SaleRepositoryDynamoDb : ISaleRepository
{
    private readonly IAmazonDynamoDB _ddb;
    private readonly string _table;

    public SaleRepositoryDynamoDb(IAmazonDynamoDB ddb, IConfiguration cfg)
    {
        _ddb = ddb;
        _table = cfg["DYNAMODB:SALES_TABLE"] ?? "Sales";
    }

    private static string Pk(string saleId) => $"SALE#{saleId}";
    private static string ReservationLookupPk(string reservationId) => $"RESLOOKUP#{reservationId}";

    public async Task CreateAsync(Sale sale, CancellationToken ct)
    {
        // Estratégia simples de idempotência sem GSI:
        // grava Sale e um "lookup item" para ReservationId (dois itens na mesma tabela).
        // (POC ok; produção: GSI em ReservationId)
        var now = DateTime.UtcNow.ToString("O");

        var saleItem = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(Pk(sale.SaleId)),
            ["SaleId"] = new AttributeValue(sale.SaleId),
            ["VehicleId"] = new AttributeValue(sale.VehicleId),
            ["CustomerId"] = new AttributeValue(sale.CustomerId),
            ["ReservationId"] = new AttributeValue(sale.ReservationId),
            ["PriceCents"] = new AttributeValue{ N = sale.PriceCents.ToString() },
            ["SoldAtUtc"] = new AttributeValue(sale.SoldAtUtc.ToString("O")),
            ["CreatedAtUtc"] = new AttributeValue(sale.CreatedAtUtc.ToString("O")),
            ["EntityType"] = new AttributeValue("SALE"),
        };

        var lookupItem = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue(ReservationLookupPk(sale.ReservationId)),
            ["ReservationId"] = new AttributeValue(sale.ReservationId),
            ["SaleId"] = new AttributeValue(sale.SaleId),
            ["CreatedAtUtc"] = new AttributeValue(now),
            ["EntityType"] = new AttributeValue("RES_LOOKUP"),
        };

        await _ddb.TransactWriteItemsAsync(new TransactWriteItemsRequest
        {
            TransactItems = new List<TransactWriteItem>
            {
                new() { Put = new Put { TableName = _table, Item = saleItem, ConditionExpression = "attribute_not_exists(PK)" } },
                new() { Put = new Put { TableName = _table, Item = lookupItem, ConditionExpression = "attribute_not_exists(PK)" } },
            }
        }, ct);
    }

    public async Task<Sale?> GetByIdAsync(string saleId, CancellationToken ct)
    {
        var res = await _ddb.GetItemAsync(_table,
            new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(Pk(saleId)) }, ct);

        if (res.Item is null || res.Item.Count == 0) return null;
        return FromItem(res.Item);
    }

    public async Task<Sale?> GetByReservationIdAsync(string reservationId, CancellationToken ct)
    {
        var lookup = await _ddb.GetItemAsync(_table,
            new Dictionary<string, AttributeValue> { ["PK"] = new AttributeValue(ReservationLookupPk(reservationId)) }, ct);

        if (lookup.Item is null || lookup.Item.Count == 0) return null;

        var saleId = lookup.Item["SaleId"].S;
        return await GetByIdAsync(saleId, ct);
    }

    public async Task<IReadOnlyList<Sale>> ListAllAsync(CancellationToken ct)
    {
        var scan = await _ddb.ScanAsync(new ScanRequest
        {
            TableName = _table,
            FilterExpression = "EntityType = :t",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":t"] = new AttributeValue("SALE")
            }
        }, ct);

        return scan.Items.Select(FromItem).ToList();
    }

    private static Sale FromItem(Dictionary<string, AttributeValue> item)
    {
        DateTime.TryParse(item["SoldAtUtc"].S, out var soldAt);
        DateTime.TryParse(item["CreatedAtUtc"].S, out var createdAt);

        return Sale.Rehydrate(
            item["SaleId"].S,
            item["VehicleId"].S,
            item["CustomerId"].S,
            item["ReservationId"].S,
            long.Parse(item["PriceCents"].N),
            soldAt == default ? DateTime.UtcNow : soldAt,
            createdAt == default ? DateTime.UtcNow : createdAt
        );
    }
}