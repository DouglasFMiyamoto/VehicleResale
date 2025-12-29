using Amazon.DynamoDBv2;
using FastEndpoints;
using InventoryService.Adapters.Persistence.DynamoDb;
using InventoryService.Core.Ports.In;
using InventoryService.Core.Ports.Out;
using InventoryService.Core.UseCases;

var b = WebApplication.CreateBuilder(args);

b.Services.AddFastEndpoints();

// DynamoDB
b.Services.AddSingleton<IAmazonDynamoDB>(_ =>
    DynamoDbClientFactory.Create(b.Configuration));

// Adapters
b.Services.AddSingleton<TablesInitializer>();
b.Services.AddSingleton<IVehicleRepository, VehicleRepositoryDynamoDb>();
b.Services.AddSingleton<IReservationRepository, ReservationRepositoryDynamoDb>();

// Infra
b.Services.AddSingleton<IClock, SystemClock>();

// UseCases
b.Services.AddSingleton<ICreateVehicleUseCase, CreateVehicleUseCase>();
b.Services.AddSingleton<IUpdateVehicleUseCase, UpdateVehicleUseCase>();
b.Services.AddSingleton<IListVehiclesUseCase, ListVehiclesUseCase>();

// TTL configurável via env RESERVATION__TTL_MINUTES
var ttl = b.Configuration.GetValue<int?>("RESERVATION:TTL_MINUTES") ?? 30;
b.Services.AddSingleton<IReserveVehicleUseCase>(sp =>
    new ReserveVehicleUseCase(
        sp.GetRequiredService<IVehicleRepository>(),
        sp.GetRequiredService<IReservationRepository>(),
        sp.GetRequiredService<IClock>(),
        ttl));

b.Services.AddSingleton<ICancelReservationUseCase, CancelReservationUseCase>();
b.Services.AddSingleton<IConfirmSaleUseCase, ConfirmSaleUseCase>();

var app = b.Build();
app.UseFastEndpoints();

// LocalStack: cria tabelas automaticamente
using (var scope = app.Services.CreateScope())
{
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var endpoint = cfg["AWS_ENDPOINT_URL"];
    if (!string.IsNullOrWhiteSpace(endpoint))
    {
        var init = scope.ServiceProvider.GetRequiredService<TablesInitializer>();
        await init.EnsureTablesAsync();
    }
}

app.Run();