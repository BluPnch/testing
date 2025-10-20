# test-local-final.ps1
param(
    [string]$TestType = "all"
)

function Write-Step([string]$message) {
    Write-Host "`n" + "="*50 -ForegroundColor Cyan
    Write-Host $message -ForegroundColor Cyan
    Write-Host "="*50 -ForegroundColor Cyan
}

function Get-PsqlPath {
    # Явный путь к psql из твоей системы
    $psqlPaths = @(
        "C:\Program Files\PostgreSQL\17\bin\psql.exe",
        "C:\Program Files\PostgreSQL\16\bin\psql.exe"
    )
    
    foreach ($path in $psqlPaths) {
        if (Test-Path $path) {
            Write-Host "✅ Using psql from: $path" -ForegroundColor Green
            return $path
        }
    }
    
    Write-Host "❌ psql.exe not found at expected locations" -ForegroundColor Red
    return $null
}

function Check-PostgreSQL {
    Write-Step "CHECKING POSTGRESQL"
    
    # Получаем путь к psql
    $psqlPath = Get-PsqlPath
    if (-not $psqlPath) {
        return $false
    }
    
    Write-Host "Testing connection to PostgreSQL on localhost:5433..." -ForegroundColor Yellow
    
    # Устанавливаем переменную окружения для пароля
    $env:PGPASSWORD = "1"
    
    # Пытаемся подключиться несколько раз
    for ($i = 1; $i -le 5; $i++) {
        Write-Host "Attempt $i..." -NoNewline
        
        try {
            $result = & $psqlPath -h localhost -p 5433 -U postgres -c "SELECT 1;" -q -t 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host " SUCCESS" -ForegroundColor Green
                Write-Host "✅ PostgreSQL is ready!" -ForegroundColor Green
                return $true
            } else {
                Write-Host " FAILED (exit code: $LASTEXITCODE)" -ForegroundColor Red
                if ($result) {
                    Write-Host "   Error: $result" -ForegroundColor DarkRed
                }
            }
        } catch {
            Write-Host " ERROR: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        if ($i -lt 5) {
            Write-Host "Waiting 3 seconds..." -ForegroundColor Gray
            Start-Sleep -Seconds 3
        }
    }
    
    Write-Host "❌ Cannot connect to PostgreSQL after 5 attempts" -ForegroundColor Red
    Write-Host "Please make sure PostgreSQL is running on port 5433" -ForegroundColor Yellow
    return $false
}

function Initialize-Database {
    Write-Step "INITIALIZING DATABASE"
    
    $psqlPath = Get-PsqlPath
    if (-not $psqlPath) {
        return
    }
    
    try {
        Write-Host "Creating database 'GreenhouseContext'..." -ForegroundColor Yellow
        & $psqlPath -h localhost -p 5433 -U postgres -c "CREATE DATABASE `"GreenhouseContext`";" -q
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Database created or already exists" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Database creation returned exit code: $LASTEXITCODE" -ForegroundColor Yellow
        }
        
        # Инициализация тестовой БД
        if (Test-Path "..\scripts\init-test-db.js") {
            Write-Host "Running init-test-db.js..." -ForegroundColor Yellow
            & node ..\scripts\init-test-db.js
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Test database initialized" -ForegroundColor Green
            } else {
                Write-Host "❌ init-test-db.js failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            }
        } else {
            Write-Host "⚠️  init-test-db.js not found at: $(Resolve-Path '..\scripts\init-test-db.js')" -ForegroundColor Yellow
        }
        
    } catch {
        Write-Host "❌ Database initialization error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Run-Dotnet-Tests {
    Write-Step "RUNNING .NET TESTS"
    
    # Сохраняем текущую директорию
    $currentDir = Get-Location
    
    try {
        # Переходим в корень проекта (на уровень выше scripts)
        Set-Location ".."
        
        Write-Host "Current directory: $(Get-Location)" -ForegroundColor Gray
        
        # Переходим в src для .NET команд
        Set-Location "src"
        
        Write-Host "Running .NET commands in: $(Get-Location)" -ForegroundColor Gray
        
        # Восстановление зависимостей
        Write-Host "Restoring dependencies..." -ForegroundColor Yellow
        & dotnet restore
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore failed with exit code: $LASTEXITCODE"
        }
        Write-Host "✅ Dependencies restored" -ForegroundColor Green
        
        # Сборка
        Write-Host "Building projects..." -ForegroundColor Yellow
        & dotnet build --verbosity minimal
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code: $LASTEXITCODE"
        }
        Write-Host "✅ Projects built successfully" -ForegroundColor Green
        
        # Создаем директорию для результатов
        New-Item -ItemType Directory -Force -Path "..\test-results" | Out-Null
        
        # Запуск тестов
        Write-Host "Running tests..." -ForegroundColor Yellow
        
        $testResult = $true
        
        switch ($TestType.ToLower()) {
            "unit" {
                Write-Host "=== UNIT TESTS ===" -ForegroundColor Cyan
                & dotnet test UnitTests\UnitTests.csproj --logger "trx;LogFileName=unit-tests.trx" --results-directory "..\test-results" --verbosity minimal
                if ($LASTEXITCODE -ne 0) { $testResult = $false }
            }
            "integration" {
                Write-Host "=== INTEGRATION TESTS ===" -ForegroundColor Cyan
                & dotnet test IntegrationTests\IntegrationTests.csproj --logger "trx;LogFileName=integration-tests.trx" --results-directory "..\test-results" --verbosity minimal
                if ($LASTEXITCODE -ne 0) { $testResult = $false }
            }
            "e2e" {
                Write-Host "=== E2E TESTS ===" -ForegroundColor Cyan
                & dotnet test E2ETests\E2ETests.csproj --logger "trx;LogFileName=e2e-tests.trx" --results-directory "..\test-results" --verbosity minimal
                if ($LASTEXITCODE -ne 0) { $testResult = $false }
            }
            "all" {
                Write-Host "=== ALL TESTS ===" -ForegroundColor Cyan
                & dotnet test --logger "trx;LogFileName=all-tests.trx" --results-directory "..\test-results" --verbosity minimal
                if ($LASTEXITCODE -ne 0) { $testResult = $false }
            }
        }
        
        if ($testResult) {
            Write-Host "✅ All tests passed!" -ForegroundColor Green
        } else {
            Write-Host "❌ Some tests failed" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    } finally {
        # Возвращаемся обратно в исходную директорию
        Set-Location $currentDir
    }
}

# Главная функция
Write-Host "🚀 LOCAL CI TEST" -ForegroundColor Magenta
Write-Host "Test Type: $TestType" -ForegroundColor Yellow
Write-Host "Working directory: $(Get-Location)" -ForegroundColor Gray

# Проверяем PostgreSQL
$postgresAvailable = Check-PostgreSQL

if ($postgresAvailable) {
    # Инициализируем БД если PostgreSQL доступен
    Initialize-Database
} else {
    Write-Host "⚠️  Skipping database tests - PostgreSQL not available" -ForegroundColor Yellow
}

# Запускаем .NET тесты (они могут работать без БД)
Run-Dotnet-Tests

Write-Step "COMPLETED"
Write-Host "🎉 Local test finished!" -ForegroundColor Green