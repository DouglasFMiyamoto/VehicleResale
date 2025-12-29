using Amazon;
using Amazon.DynamoDBv2;

namespace Infra.DynamoDb;

public static class DynamoDbClientFactory
{
    public static IAmazonDynamoDB Create(IConfiguration cfg)
    {
        var region = cfg["AWS_REGION"] ?? "us-east-1";
        var endpointUrl = cfg["AWS_ENDPOINT_URL"];

        var config = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(region)
        };

        if (!string.IsNullOrWhiteSpace(endpointUrl))
            config.ServiceURL = endpointUrl; // LocalStack

        return new AmazonDynamoDBClient(
            cfg["AWS_ACCESS_KEY_ID"] ?? "test",
            cfg["AWS_SECRET_ACCESS_KEY"] ?? "test",
            config);
    }
}
