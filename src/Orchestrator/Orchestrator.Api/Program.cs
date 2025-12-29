using FastEndpoints;
using Orchestrator.Adapters.Integration;
using Orchestrator.Core.Ports.In;
using Orchestrator.Core.Ports.Out;
using Orchestrator.Core.UseCases;

var b = WebApplication.CreateBuilder(args);

b.Services.AddFastEndpoints();

// Http clients
b.Services.AddHttpClient<ICustomerClient, CustomerHttpClient>(c =>
    c.BaseAddress = new Uri(b.Configuration["CUSTOMER_BASE_URL"]!));

b.Services.AddHttpClient<IInventoryClient, InventoryHttpClient>(c =>
    c.BaseAddress = new Uri(b.Configuration["INVENTORY_BASE_URL"]!));

b.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(c =>
    c.BaseAddress = new Uri(b.Configuration["PAYMENT_BASE_URL"]!));

b.Services.AddHttpClient<ISalesClient, SalesHttpClient>(c =>
    c.BaseAddress = new Uri(b.Configuration["SALES_BASE_URL"]!));

// UseCases
b.Services.AddSingleton<IStartPurchaseUseCase, StartPurchaseUseCase>();
b.Services.AddSingleton<IFinalizePurchaseUseCase, FinalizePurchaseUseCase>();

var app = b.Build();
app.UseFastEndpoints();
app.Run();