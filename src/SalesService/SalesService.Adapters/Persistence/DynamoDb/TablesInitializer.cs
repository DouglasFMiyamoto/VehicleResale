using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;

namespace SalesService.Adapters.Persistence.DynamoDb;

public sealed class TablesInitializer
{
    private readonly IAmazonDynamoDB _ddb;
    private readonly IConfiguration _cfg;

    public TablesInitializer(IAmazonDynamoDB ddb, IConfiguration cfg)
    {
        _ddb = ddb;
        _cfg = cfg;
    }

    public async Task EnsureSalesTableAsync(CancellationToken ct = default)
    {
        var tableName = _cfg["DYNAMODB:SALES_TABLE"] ?? "Sales";

        var tables = await _ddb.ListTablesAsync(new ListTablesRequest(), ct);
        if (tables.TableNames.Contains(tableName)) return;

        await _ddb.CreateTableAsync(new CreateTableRequest
        {
            TableName = tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = new List<AttributeDefinition> { new("PK", ScalarAttributeType.S) },
            KeySchema = new List<KeySchemaElement> { new("PK", KeyType.HASH) }
        }, ct);
    }
}