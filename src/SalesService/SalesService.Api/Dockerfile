FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SalesService.Api/SalesService.Api.csproj SalesService.Api/
COPY SalesService.Core/SalesService.Core.csproj SalesService.Core/
COPY SalesService.Adapters/SalesService.Adapters.csproj SalesService.Adapters/
RUN dotnet restore SalesService.Api/SalesService.Api.csproj

COPY . .
RUN dotnet publish SalesService.Api/SalesService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesService.Api.dll"]