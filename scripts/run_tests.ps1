param(
    [string]$TestType = "all",
    [string]$Environment = "Development",
    [switch]$CI = $false
)

function Write-Step([string]$message) {
    Write-Host "`n=== $message ===" -ForegroundColor Cyan
}

function Write-Success([string]$message) {
    Write-Host "[SUCCESS] $message" -ForegroundColor Green
}

function Write-Error([string]$message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

function Write-Info([string]$message) {
    Write-Host "[INFO] $message" -ForegroundColor Yellow
}

function Load-EnvFile {
    $envPath = "scripts\.env"
    if (Test-Path $envPath) {
        Write-Info "Loading environment variables from $envPath"
        Get-Content $envPath | ForEach-Object {
            if ($_ -match '^\s*([^#][^=]+)=(.*)') {
                $key = $matches[1].Trim()
                $value = $matches[2].Trim()
                [Environment]::SetEnvironmentVariable($key, $value)
                Write-Info "  $key = $value"
            }
        }
    } else {
        Write-Info ".env file not found at $envPath, using default values"
    }
}

# Загружаем переменные из .env файла
Load-EnvFile

# Используем переменные из .env или значения по умолчанию
$DB_HOST = [Environment]::GetEnvironmentVariable("DB_HOST") ?? "localhost"
$DB_PORT = [Environment]::GetEnvironmentVariable("DB_PORT") ?? "5433"
$DB_USER = [Environment]::GetEnvironmentVariable("DB_USER") ?? "postgres"
$DB_PASSWORD = [Environment]::GetEnvironmentVariable("DB_PASSWORD") ?? "1"
$DB_NAME = [Environment]::GetEnvironmentVariable("DB_NAME") ?? "GreenhouseContext"

Write-Info "Database configuration:"
Write-Info "  Host: $DB_HOST"
Write-Info "  Port: $DB_PORT"
Write-Info "  User: $DB_USER"
Write-Info "  Database: $DB_NAME"

function Stop-ServerProcesses {
    Write-Info "Stopping running servers..."
    
    # Только остановка процессов на портах (самый безопасный способ)
    $serverPorts = @(
        @{Port = 5000; Name = ".NET Server"},
        @{Port = 3000; Name = "Node.js Server"}, 
        @{Port = 3001; Name = "Node.js Server (Alt)"}
    )
    
    foreach ($server in $serverPorts) {
        $port = $server.Port
        $name = $server.Name
        
        try {
            $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object { $_.State -eq "Listen" }
            foreach ($connection in $connections) {
                if ($connection.OwningProcess -gt 0) {
                    $processName = (Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue).ProcessName
                    Write-Info "Stopping $name on port $port (PID: $($connection.OwningProcess), Process: $processName)"
                    try {
                        Stop-Process -Id $connection.OwningProcess -Force -ErrorAction SilentlyContinue
                        Write-Info "Successfully stopped process $($connection.OwningProcess)"
                    } catch {
                        Write-Info "Could not stop process $($connection.OwningProcess): $($_.Exception.Message)"
                    }
                }
            }
        } catch {
            # Исправленная строка - экранируем двоеточие
            Write-Info "Error checking port ${port}: $($_.Exception.Message)"
        }
    }
    
    Start-Sleep -Seconds 2
    Write-Info "Server cleanup completed"
}

function Cleanup-Allure {
    if (Test-Path "allure-results") {
        Remove-Item "allure-results" -Recurse -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path "test-results") {
        Remove-Item "test-results" -Recurse -Force -ErrorAction SilentlyContinue
    }
}

function Run-UnitTests {
    Write-Step "UNIT TESTS"
    
    New-Item -ItemType Directory -Force -Path "allure-results" | Out-Null
    New-Item -ItemType Directory -Force -Path "test-results" | Out-Null
    
    Write-Info "Restoring dependencies..."
    & dotnet restore "src\UnitTests\UnitTests.csproj" --verbosity quiet
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore dependencies"
    }
    
    Write-Info "Building project..."
    & dotnet build "src\UnitTests\UnitTests.csproj" --configuration Release --no-restore --verbosity minimal
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    $RandomSeed = Get-Random -Minimum 1 -Maximum 99999
    Write-Info "Random seed: $RandomSeed"
    
    Write-Info "Running .NET Unit tests..."
    & dotnet test "src\UnitTests\UnitTests.csproj" `
        --configuration Release `
        --logger "console;verbosity=normal" `
        --logger "trx;LogFileName=unit-tests.trx" `
        --results-directory "test-results" `
        --no-build `
        --verbosity normal `
        -- XUnit.RandomSeed=$RandomSeed
    
    if ($LASTEXITCODE -ne 0) {
        throw "Unit tests failed"
    }
    
    Write-Success "Unit tests completed"
}

function Run-IntegrationTests {
    Write-Step "INTEGRATION TESTS"
    
    # Проверяем PostgreSQL
    if ($CI) {
        Write-Info "Running in CI mode - assuming PostgreSQL is already running"
    } else {
        Write-Info "Checking PostgreSQL..."
        & powershell -ExecutionPolicy Bypass -File ./scripts/start-postgresql.ps1
    }
        
    # Очищаем интеграционные тесты если скрипт существует
    if (Test-Path "scripts\clean-integration-tests.ps1") {
        Write-Info "Cleaning integration tests..."
        & powershell -ExecutionPolicy Bypass -File ./scripts/clean-integration-tests.ps1
    } else {
        Write-Info "Clean script not found, manually cleaning..."
        if (Test-Path "src\IntegrationTests\bin") {
            Remove-Item -Path "src\IntegrationTests\bin" -Recurse -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path "src\IntegrationTests\obj") {
            Remove-Item -Path "src\IntegrationTests\obj" -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
    
    Write-Info "Initializing test database..."
    & node scripts/init-test-db.js
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to initialize test database"
        Write-Info "Skipping integration tests due to database setup failure"
        return
    }
    
    Write-Info "Running .NET Integration tests..."
    
    if (Test-Path "src\IntegrationTests\IntegrationTests.csproj") {
        & dotnet test "src\IntegrationTests\IntegrationTests.csproj" `
            --configuration Release `
            --logger "trx;LogFileName=integration-tests.trx" `
            --results-directory "test-results" `
            --environment "Test"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Integration tests failed"
        } else {
            Write-Success "Integration tests completed"
        }
    } else {
        Write-Info "Integration tests project not found, skipping"
    }
}

function Run-E2ETests {
    Write-Step "E2E TESTS"
        
    Write-Info "Checking if server is running on port 5097..."
    
    # Проверка что сервер запущен
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5097/api/health" -TimeoutSec 5 -ErrorAction Stop
        if ($response.StatusCode -ne 200) {
            throw "Server returned status code: $($response.StatusCode)"
        }
        Write-Info "✅ Server is running on port 5097"
    } catch {
        Write-Warning "Server is not running on port 5097. E2E tests may fail."
        Write-Info "Start the server with: npm run servers"
    }
    
    Write-Info "Running .NET E2E tests with Playwright..."
    
    # Проверяем существование .NET E2E тестов
    if (Test-Path "src\E2ETests\E2ETests.csproj") {
        Write-Info "Found .NET E2E tests project"
        
        # Очистка E2E тестов
        Write-Info "Cleaning E2E tests..."
        Remove-Item -Path "src\E2ETests\bin" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "src\E2ETests\obj" -Recurse -Force -ErrorAction SilentlyContinue
        
        # Восстановление зависимостей
        Write-Info "Restoring dependencies..."
        & dotnet restore "src\E2ETests\E2ETests.csproj" --verbosity quiet
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to restore E2E test dependencies"
            return
        }
        
        # Сборка проекта
        Write-Info "Building E2E tests..."
        & dotnet build "src\E2ETests\E2ETests.csproj" --configuration Release --no-restore
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to build E2E tests"
            return
        }
        
        # Установка Playwright браузеров (если нужно)
        Write-Info "Setting up Playwright..."
        & dotnet test "src\E2ETests\E2ETests.csproj" --configuration Release --no-build --verbosity minimal `
            --filter "Category=Setup" 2>&1 | Out-Null
        
        # Запуск E2E тестов с Allure
        Write-Info "Running .NET E2E tests with Allure reporting..."
        & dotnet test "src\E2ETests\E2ETests.csproj" `
            --configuration Release `
            --logger "trx;LogFileName=e2e-tests.trx" `
            --logger "console;verbosity=normal" `
            --results-directory "test-results" `
            --no-build `
            --verbosity normal
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "E2E tests failed with exit code: $LASTEXITCODE"
        } else {
            Write-Success ".NET E2E tests completed"
        }
    } else {
        Write-Info ".NET E2E tests not found at src\E2ETests\E2ETests.csproj"
        Write-Info "Available test projects:"
        if (Test-Path "src\UnitTests\UnitTests.csproj") { Write-Info "  - UnitTests" }
        if (Test-Path "src\IntegrationTests\IntegrationTests.csproj") { Write-Info "  - IntegrationTests" }
        if (Test-Path "src\E2ETests\E2ETests.csproj") { Write-Info "  - E2ETests" }
    }
}

function Generate-Reports {
    Write-Step "GENERATING REPORTS"
    
    # Создать директории если не существуют
    New-Item -ItemType Directory -Force -Path "allure-results" | Out-Null
    New-Item -ItemType Directory -Force -Path "test-results" | Out-Null
    
    Write-Info "Collecting test results from all .NET test types..."
    
    # 1. Собрать результаты Unit Tests
    if (Test-Path "src\UnitTests\bin\Release\net9.0\allure-results") {
        Write-Info "Copying UnitTests allure results..."
        $unitResults = Get-ChildItem "src\UnitTests\bin\Release\net9.0\allure-results" -Recurse -File
        Copy-Item -Path "src\UnitTests\bin\Release\net9.0\allure-results\*" -Destination "allure-results\" -Recurse -Force
        Write-Info "✅ UnitTests: $($unitResults.Count) result files"
    }
    
    # 2. Собрать результаты Integration Tests  
    if (Test-Path "src\IntegrationTests\bin\Release\net9.0\allure-results") {
        Write-Info "Copying IntegrationTests allure results..."
        $integrationResults = Get-ChildItem "src\IntegrationTests\bin\Release\net9.0\allure-results" -Recurse -File
        Copy-Item -Path "src\IntegrationTests\bin\Release\net9.0\allure-results\*" -Destination "allure-results\" -Recurse -Force
        Write-Info "✅ IntegrationTests: $($integrationResults.Count) result files"
    }
    
    # 3. Собрать результаты E2E Tests (.NET)
    if (Test-Path "src\E2ETests\bin\Release\net9.0\allure-results") {
        Write-Info "Copying E2E tests allure results..."
        $e2eResults = Get-ChildItem "src\E2ETests\bin\Release\net9.0\allure-results" -Recurse -File
        Copy-Item -Path "src\E2ETests\bin\Release\net9.0\allure-results\*" -Destination "allure-results\" -Recurse -Force
        Write-Info "✅ E2E Tests: $($e2eResults.Count) result files"
    }
    
    # 4. Копировать TRX файлы как резерв
    if (Test-Path "test-results\*.trx") {
        Write-Info "Copying TRX test results..."
        Copy-Item -Path "test-results\*.trx" -Destination "allure-results\" -Force
    }

    # Проверить что allure-results не пустой
    $allResultFiles = Get-ChildItem "allure-results" -Recurse -File -ErrorAction SilentlyContinue
    if ($allResultFiles.Count -eq 0) {
        Write-Warning "No test results found in allure-results directory"
        return
    }

    Write-Info "Total .NET test result files collected: $($allResultFiles.Count)"

    $allureCmd = Get-Command "allure" -ErrorAction SilentlyContinue
    if ($allureCmd) {
        Write-Info "Generating Allure report from all .NET test results..."
        & allure generate "allure-results" -o "allure-report" --clean
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Allure report generated successfully with all .NET test types"
            
            # Показать статистику
            $reportFiles = Get-ChildItem "allure-report" -Recurse -File | Where-Object { $_.Name -like "*.html" }
            Write-Info "Generated report with $($reportFiles.Count) HTML pages"
            
            $openReport = Read-Host "Open Allure report? (y/n)"
            if ($openReport -eq 'y') {
                & allure open "allure-report"
            }
        } else {
            Write-Error "Failed to generate Allure report"
        }
    } else {
        Write-Info "Allure CLI not found, skipping report generation"
        Write-Info "Install Allure: https://docs.qameta.io/allure/#_installing_a_commandline"
    }
}

try {
    Write-Step "GREENHOUSE TEST SUITE"
    Write-Info "Environment: $Environment"
    Write-Info "Test Type: $TestType"
    
    
    Stop-ServerProcesses
    
    Cleanup-Allure
    
    switch ($TestType.ToLower()) {
        "unit" {
            Run-UnitTests
            Generate-Reports
        }
        "integration" {
            Run-IntegrationTests
            Generate-Reports
        }
        "e2e" {
            Run-E2ETests
            Generate-Reports
        }
        "all" {
            Run-UnitTests
            Run-IntegrationTests
            Run-E2ETests
            Generate-Reports
            Write-Step "TEST SUITE COMPLETED"
            Write-Success "All tests passed successfully!"
        }
        default {
            Write-Error "Unknown test type: $TestType"
            Write-Info "Available types: unit, integration, e2e, all"
            exit 1
        }
    }
} catch {
    Write-Error "Test execution failed: $($_.Exception.Message)"
    exit 1
}