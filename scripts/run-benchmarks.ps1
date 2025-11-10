param(
    [string]$Target = "all",
    [switch]$WithMonitoring = $true
)

if ($WithMonitoring) {
    Write-Host "Starting monitoring infrastructure..." -ForegroundColor Green
    docker-compose up -d prometheus grafana cadvisor
    
    Write-Host "Grafana available at: http://localhost:3001" -ForegroundColor Cyan
    Write-Host "Prometheus available at: http://localhost:9090" -ForegroundColor Cyan
}

Write-Host "Building and running benchmarks..." -ForegroundColor Green

# Останавливаем и пересобираем контейнеры
docker-compose stop benchmark-net8 benchmark-net9
docker-compose build --no-cache

if ($Target -eq "all" -or $Target -eq "net8") {
    Write-Host "`nRunning .NET 8 benchmarks..." -ForegroundColor Yellow
    docker-compose run --rm benchmark-net8
}

if ($Target -eq "all" -or $Target -eq "net9") {
    Write-Host "`nRunning .NET 9 benchmarks..." -ForegroundColor Yellow
    docker-compose run --rm benchmark-net9
}

Write-Host "`nBenchmark completed!" -ForegroundColor Green
Write-Host "Check Grafana for real-time metrics: http://localhost:3001" -ForegroundColor Cyan
Write-Host "Check Prometheus for raw data: http://localhost:9090" -ForegroundColor Cyan