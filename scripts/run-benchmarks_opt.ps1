param(
    [string]$Target = "all",
    [switch]$WithMonitoring = $true,
    [switch]$Fast = $false  # Быстрый режим без сборки
)

if ($WithMonitoring) {
    Write-Host "Starting monitoring infrastructure..." -ForegroundColor Green
    docker-compose up -d prometheus grafana cadvisor
    Write-Host "Grafana: http://localhost:3001" -ForegroundColor Cyan
}

if (-not $Fast) {
    Write-Host "Building containers..." -ForegroundColor Yellow
    docker-compose stop benchmark-net8 benchmark-net9
    docker-compose build benchmark-net8 benchmark-net9
} else {
    Write-Host "Fast mode - skipping build..." -ForegroundColor Green
}

Write-Host "Running benchmarks..." -ForegroundColor Green

if ($Target -eq "all" -or $Target -eq "net8") {
    Write-Host "`nRunning .NET 8 benchmarks..." -ForegroundColor Yellow
    docker-compose run --rm benchmark-net8
}

if ($Target -eq "all" -or $Target -eq "net9") {
    Write-Host "`nRunning .NET 9 benchmarks..." -ForegroundColor Yellow
    docker-compose run --rm benchmark-net9
}

Write-Host "`nBenchmark completed!" -ForegroundColor Green
Write-Host "`nCollection Passes Results:" -ForegroundColor Cyan
docker-compose logs benchmark-net8 | findstr "Collection Passes"
docker-compose logs benchmark-net9 | findstr "Collection Passes"