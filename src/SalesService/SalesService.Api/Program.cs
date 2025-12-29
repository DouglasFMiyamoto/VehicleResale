using Amazon.DynamoDBv2;
using FastEndpoints;
using SalesService.Adapters.Persistence.DynamoDb;
using SalesService.Core.Ports.In;
using SalesService.Core.Ports.Out;
using SalesService.Core.UseCases;

var b = WebApplication.CreateBuilder(args);

b.Services.AddFastEndpoints();

// DynamoDB
b.Services.AddSingleton<IAmazonDynamoDB>(_ =>
    DynamoDbClientFactory.Create(b.Configuration));

// Adapters
b.Services.AddSingleton<TablesInitializer>();
b.Services.AddSingleton<ISaleRepository, SaleRepositoryDynamoDb>();

// UseCases
b.Services.AddSingleton<ICreateSaleUseCase, CreateSaleUseCase>();
b.Services.AddSingleton<IGetSaleUseCase, GetSaleUseCase>();
b.Services.AddSingleton<IListSalesUseCase, ListSalesUseCase>();

var app = b.Build();
app.UseFastEndpoints();

// LocalStack: cria tabela automaticamente
using (var scope = app.Services.CreateScope())
{
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var endpoint = cfg["AWS_ENDPOINT_URL"];
    if (!string.IsNullOrWhiteSpace(endpoint))
    {
        var init = scope.ServiceProvider.GetRequiredService<TablesInitializer>();
        await init.EnsureSalesTableAsync();
    }
}

app.Run();