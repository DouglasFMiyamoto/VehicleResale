using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace InventoryService.Adapters.Persistence.DynamoDb;

public static class DynamoDbClientFactory
{
    public static IAmazonDynamoDB Create(IConfiguration cfg)
    {
        var region = cfg["AWS_REGION"] ?? "us-east-1";
        var endpoint = cfg["AWS_ENDPOINT_URL"]; // LocalStack

        var ddbConfig = new AmazonDynamoDBConfig
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
        };

        if (!string.IsNullOrWhiteSpace(endpoint))
            ddbConfig.ServiceURL = endpoint;

        var accessKey = cfg["AWS_ACCESS_KEY_ID"] ?? "test";
        var secretKey = cfg["AWS_SECRET_ACCESS_KEY"] ?? "test";
        var creds = new BasicAWSCredentials(accessKey, secretKey);

        return new AmazonDynamoDBClient(creds, ddbConfig);
    }
}