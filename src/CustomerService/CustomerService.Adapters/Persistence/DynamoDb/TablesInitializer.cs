using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;

namespace CustomerService.Adapters.Persistence.DynamoDb;

public class TablesInitializer(IAmazonDynamoDB ddb, IConfiguration cfg)
{
    public async Task EnsureCustomersTableAsync()
    {
        var table = cfg["CUSTOMERS_TABLE"] ?? "Customers";

        if (await ExistsAsync(table)) return;

        await ddb.CreateTableAsync(new CreateTableRequest
        {
            TableName = table,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = [ new("PK", ScalarAttributeType.S) ],
            KeySchema = [ new("PK", KeyType.HASH) ]
        });

        await WaitActiveAsync(table);
    }

    private async Task<bool> ExistsAsync(string table)
    {
        try { await ddb.DescribeTableAsync(table); return true; }
        catch (ResourceNotFoundException) { return false; }
    }

    private async Task WaitActiveAsync(string table)
    {
        for (int i = 0; i < 40; i++)
        {
            var desc = await ddb.DescribeTableAsync(table);
            if (desc.Table.TableStatus == TableStatus.ACTIVE) return;
            await Task.Delay(250);
        }
    }
}