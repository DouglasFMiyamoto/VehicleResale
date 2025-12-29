FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# csproj (ajuste os nomes se seus projetos tiverem nomes diferentes)
COPY Orchestrator.Api/Orchestrator.Api.csproj Orchestrator.Api/
COPY Orchestrator.Core/Orchestrator.Core.csproj Orchestrator.Core/
RUN dotnet restore Orchestrator.Api/Orchestrator.Api.csproj

COPY . .
RUN dotnet publish Orchestrator.Api/Orchestrator.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Orchestrator.Api.dll"]