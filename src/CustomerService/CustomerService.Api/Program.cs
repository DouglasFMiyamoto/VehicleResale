using Amazon.DynamoDBv2;
using CustomerService.Adapters.Persistence.DynamoDb;
using CustomerService.Core.Ports.In;
using CustomerService.Core.Ports.Out;
using CustomerService.Core.UseCases;
using FastEndpoints;
using CustomerService.Adapters.Security.AesGcm;

var b = WebApplication.CreateBuilder(args);

b.Services.AddFastEndpoints();

b.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    return DynamoDbClientFactory.Create(cfg);
});

b.Services.AddSingleton<ICustomerDataProtector, AesGcmCustomerDataProtector>();

// Adapters
b.Services.AddSingleton<TablesInitializer>();
b.Services.AddSingleton<ICustomerRepository, CustomerRepositoryDynamoDb>();

// UseCases (Ports.In)
b.Services.AddSingleton<ICreateCustomerUseCase, CreateCustomerUseCase>();
b.Services.AddSingleton<IGetCustomerUseCase, GetCustomerUseCase>();
b.Services.AddSingleton<IExistsCustomerUseCase, ExistsCustomerUseCase>();

var app = b.Build();
app.UseFastEndpoints();

using (var scope = app.Services.CreateScope())
{
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var endpoint = cfg["AWS_ENDPOINT_URL"]; 
    if (!string.IsNullOrWhiteSpace(endpoint))
    {
        var init = scope.ServiceProvider.GetRequiredService<TablesInitializer>();
        await init.EnsureCustomersTableAsync();
    }
}

app.Run();
