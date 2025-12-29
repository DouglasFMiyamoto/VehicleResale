FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY InventoryService.Api/InventoryService.Api.csproj InventoryService.Api/
COPY InventoryService.Core/InventoryService.Core.csproj InventoryService.Core/
COPY InventoryService.Adapters/InventoryService.Adapters.csproj InventoryService.Adapters/
RUN dotnet restore InventoryService.Api/InventoryService.Api.csproj

COPY . .
RUN dotnet publish InventoryService.Api/InventoryService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "InventoryService.Api.dll"]