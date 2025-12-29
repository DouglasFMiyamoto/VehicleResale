using Amazon.DynamoDBv2;
using FastEndpoints;
using PaymentService.Adapters.Persistence.DynamoDb;
using PaymentService.Core.Ports.In;
using PaymentService.Core.Ports.Out;
using PaymentService.Core.UseCases;

var b = WebApplication.CreateBuilder(args);

b.Services.AddFastEndpoints();

// DynamoDB
b.Services.AddSingleton<IAmazonDynamoDB>(_ =>
    DynamoDbClientFactory.Create(b.Configuration));

// Adapters
b.Services.AddSingleton<TablesInitializer>();
b.Services.AddSingleton<IPaymentRepository, PaymentRepositoryDynamoDb>();

// UseCases
b.Services.AddSingleton<ICreatePaymentUseCase, CreatePaymentUseCase>();
b.Services.AddSingleton<IGetPaymentUseCase, GetPaymentUseCase>();
b.Services.AddSingleton<IMarkPaymentPaidUseCase, MarkPaymentPaidUseCase>();

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
        await init.EnsurePaymentsTableAsync();
    }
}

app.Run();