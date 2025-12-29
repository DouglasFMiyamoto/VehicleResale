FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PaymentService.Api/PaymentService.Api.csproj PaymentService.Api/
COPY PaymentService.Core/PaymentService.Core.csproj PaymentService.Core/
COPY PaymentService.Adapters/PaymentService.Adapters.csproj PaymentService.Adapters/
RUN dotnet restore PaymentService.Api/PaymentService.Api.csproj

COPY . .
RUN dotnet publish PaymentService.Api/PaymentService.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]