#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

Write-Host "=== LINQ Performance Benchmark with Docker ===" -ForegroundColor Green

# Переходим в корневую директорию проекта
Set-Location -Path "C:\sem7\testing"

# Создаем директории
$resultsDir = "results"
if (!(Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir -Force
    Write-Host "Created results directory" -ForegroundColor Green
}

# Проверяем, что Docker работает
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker version
    if ($LASTEXITCODE -ne 0) {
        throw "Docker is not running"
    }
    Write-Host "Docker is running!" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Docker is not available. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Останавливаем и удаляем предыдущие контейнеры
Write-Host "Cleaning up previous containers..." -ForegroundColor Yellow
docker-compose down --remove-orphans

# Собираем образы
Write-Host "Building Docker images..." -ForegroundColor Cyan
docker-compose build

# Запускаем Prometheus и cAdvisor
Write-Host "Starting monitoring services..." -ForegroundColor Cyan
docker-compose up -d prometheus cadvisor

# Даем время запуститься
Write-Host "Waiting for services to start..." -ForegroundColor Gray
Start-Sleep -Seconds 10

for ($i = 1; $i -le 5; $i++) {
    Write-Host "`n=== Running iteration $i ===" -ForegroundColor Cyan
    
    try {
        # NET 8
        Write-Host "Testing .NET 8.0..." -ForegroundColor Yellow
        docker-compose run --rm benchmark-net8
        
        # NET 9
        Write-Host "Testing .NET 9.0..." -ForegroundColor Yellow  
        docker-compose run --rm benchmark-net9
        
        Write-Host "Iteration $i completed successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Error in iteration $i : $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Очистка контейнеров
    docker container prune -f
    
    # Пауза между прогонами
    if ($i -lt 5) {
        Write-Host "Waiting 2 seconds before next iteration..." -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
}

Write-Host "`n=== Benchmark completed! ===" -ForegroundColor Green
Write-Host "Results: C:\sem7\testing\results\" -ForegroundColor Yellow
Write-Host "Prometheus: http://localhost:9090" -ForegroundColor Yellow
Write-Host "cAdvisor: http://localhost:8089" -ForegroundColor Yellow

# Останавливаем сервисы
Write-Host "Stopping monitoring services..." -ForegroundColor Cyan
docker-compose down