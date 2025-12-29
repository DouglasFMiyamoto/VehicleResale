using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;

namespace CustomerService.Adapters.Persistence.DynamoDb;

public static class DynamoDbClientFactory
{
    public static IAmazonDynamoDB Create(IConfiguration cfg)
    {
        var region = cfg["AWS_REGION"] ?? "us-east-1";
        var endpoint = cfg["AWS_ENDPOINT_URL"]; // LocalStack

        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
        };

        if (!string.IsNullOrWhiteSpace(endpoint))
            config.ServiceURL = endpoint;

        return new AmazonDynamoDBClient(config);
    }
}