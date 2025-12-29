using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Infra.DynamoDb;

public class TablesInitializer(IAmazonDynamoDB ddb)
{
    public async Task EnsureSimplePkTableAsync(string tableName)
    {
        if (await ExistsAsync(tableName)) return;

        await ddb.CreateTableAsync(new CreateTableRequest
        {
            TableName = tableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = [new("PK", ScalarAttributeType.S)],
            KeySchema = [new("PK", KeyType.HASH)]
        });

        await WaitActiveAsync(tableName);
    }

    private async Task<bool> ExistsAsync(string table)
    {
        try { await ddb.DescribeTableAsync(table); return true; }
        catch (ResourceNotFoundException) { return false; }
    }

    private async Task WaitActiveAsync(string table)
    {
        for (var i = 0; i < 40; i++)
        {
            var desc = await ddb.DescribeTableAsync(table);
            if (desc.Table.TableStatus == TableStatus.ACTIVE) return;
            await Task.Delay(250);
        }
    }
}
