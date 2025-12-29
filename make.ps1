param(
    [string]$command,
    [string]$service
)

function Show-Help {
    Write-Host ""
    Write-Host "Uso:"
    Write-Host "  .\make.ps1 up                -> sobe todos os serviços"
    Write-Host "  .\make.ps1 down              -> derruba todos os serviços"
    Write-Host "  .\make.ps1 rebuild           -> rebuild completo"
    Write-Host "  .\make.ps1 logs <service>    -> logs de um serviço"
    Write-Host "  .\make.ps1 ps                -> status dos containers"
    Write-Host ""
    Write-Host "Serviços disponíveis:"
    Write-Host "  localstack"
    Write-Host "  orchestrator-api"
    Write-Host "  customer-api"
    Write-Host "  inventory-api"
    Write-Host "  sales-api"
    Write-Host "  payment-api"
    Write-Host ""
}

switch ($command) {

    "up" {
        Write-Host "Subindo containers..."
        docker compose up -d
    }

    "down" {
        Write-Host "Derrubando containers..."
        docker compose down
    }

    "rebuild" {
        Write-Host "Rebuild completo..."
        docker compose down
        docker compose build --no-cache
        docker compose up -d
    }

    "logs" {
        if (-not $service) {
            Write-Host "Informe o nome do serviço"
            Show-Help
            exit 1
        }
        docker compose logs -f $service
    }

    "ps" {
        docker compose ps
    }

    default {
        Show-Help
    }
}