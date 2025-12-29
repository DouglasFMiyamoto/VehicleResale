# =========================
# Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Copia csproj (Api + Core + Adapters) para aproveitar cache do restore
COPY CustomerService.Api/CustomerService.Api.csproj CustomerService.Api/
COPY CustomerService.Core/CustomerService.Core.csproj CustomerService.Core/
COPY CustomerService.Adapters/CustomerService.Adapters.csproj CustomerService.Adapters/

# 2) Restore no csproj da API (que referencia Core/Adapters)
RUN dotnet restore CustomerService.Api/CustomerService.Api.csproj

# 3) Copia o restante do código
COPY . .

# 4) Publish
RUN dotnet publish CustomerService.Api/CustomerService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# =========================
# Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CustomerService.Api.dll"]