function Write-Step([string]$message) {
    Write-Host "`n=== $message ===" -ForegroundColor Cyan
}

function Write-Info([string]$message) {
    Write-Host "[INFO] $message" -ForegroundColor Yellow
}

function Write-Success([string]$message) {
    Write-Host "[SUCCESS] $message" -ForegroundColor Green
}

function Write-Error([string]$message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

function Stop-PortProcess($port) {
    Write-Info "Checking port $port..."
    
    $processStopped = $false
    
    # Метод 1: Используем Get-NetTCPConnection (более надежный)
    try {
        $connections = @(Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object { $_.State -eq "Listen" })
        foreach ($connection in $connections) {
            if ($connection.OwningProcess -gt 0) {
                $processId = $connection.OwningProcess
                $processName = (Get-Process -Id $processId -ErrorAction SilentlyContinue).ProcessName
                Write-Info "Stopping process '$processName' on port $port (PID: $processId)"
                try {
                    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                    $processStopped = $true
                    Write-Info "Successfully stopped process $processId"
                } catch {
                    Write-Info "Could not stop process $processId gracefully, trying taskkill..."
                    taskkill /PID $processId /F 2>$null
                    $processStopped = $true
                }
                Start-Sleep -Seconds 1
            }
        }
    } catch {
        Write-Info "Get-NetTCPConnection method failed: $($_.Exception.Message)"
    }
    
    # Метод 2: Резервный метод через netstat
    try {
        $processInfo = netstat -ano | findstr ":$port" | findstr "LISTENING"
        if ($processInfo) {
            foreach ($line in $processInfo) {
                $parts = $line -split '\s+'
                $processId = $parts[-1]
                if ($processId -and $processId -ne "0" -and $processId -ne "PID") {
                    Write-Info "Stopping process on port $port (PID: $processId) via netstat"
                    try {
                        taskkill /PID $processId /F 2>$null
                        $processStopped = $true
                        Write-Info "Successfully stopped process $processId"
                    } catch {
                        Write-Info "Failed to stop process $processId"
                    }
                    Start-Sleep -Seconds 1
                }
            }
        }
    } catch {
        Write-Info "Netstat method failed: $($_.Exception.Message)"
    }
    
    # Метод 3: Принудительно завершаем все Node.js процессы
    try {
        $nodeProcesses = Get-Process node -ErrorAction SilentlyContinue
        if ($nodeProcesses) {
            Write-Info "Stopping all Node.js processes"
            Stop-Process -Name node -Force -ErrorAction SilentlyContinue
            $processStopped = $true
            Start-Sleep -Seconds 2
        }
    } catch {
        Write-Info "No Node.js processes found or couldn't stop them"
    }
    
    if (-not $processStopped) {
        Write-Info "No processes found on port $port"
    }
    
    # Двойная проверка
    Start-Sleep -Seconds 2
}

function Test-PortAvailable($port) {
    try {
        $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Where-Object { $_.State -eq "Listen" }
        if ($connection) {
            Write-Error "Port $port is still in use by PID: $($connection.OwningProcess)"
            return $false
        } else {
            Write-Info "Port $port is available"
            return $true
        }
    } catch {
        Write-Info "Port $port appears to be available"
        return $true
    }
}

# Проверка путей (после объявления функций)
Write-Info "Checking project structure..."

# Правильные пути относительно C:\sem7\testing
$solutionPath = "src/Greenhouse.sln"
$projectPath = "src/Server/Server.csproj"
$serverJsPath = "src/server.js"

if (-not (Test-Path $solutionPath)) {
    Write-Error "Solution file not found: $solutionPath"
    Write-Info "Current directory: $PWD"
    Write-Info "Looking for solution files..."
    Get-ChildItem -Recurse -Filter "*.sln" | ForEach-Object { Write-Info " - $($_.FullName)" }
    throw "Solution file not found"
}

if (-not (Test-Path $projectPath)) {
    Write-Error "Project file not found: $projectPath"
    Write-Info "Looking for project files..."
    Get-ChildItem -Recurse -Filter "*.csproj" | ForEach-Object { Write-Info " - $($_.FullName)" }
    throw "Project file not found"
}

if (-not (Test-Path $serverJsPath)) {
    Write-Error "Server.js file not found: $serverJsPath"
    Write-Info "Looking for server.js..."
    Get-ChildItem -Recurse -Filter "server.js" | ForEach-Object { Write-Info " - $($_.FullName)" }
    throw "server.js file not found"
}

Write-Success "Project structure verified"

try {
    Write-Step "GREENHOUSE SYSTEM STARTUP"
    
    # Остановка процессов
    Stop-PortProcess 3000  # Node.js
    Stop-PortProcess 5000  # .NET
    Stop-PortProcess 5433  # PostgreSQL

    # Проверка наличия node_modules
    if (-not (Test-Path "node_modules")) {
        Write-Info "Installing npm packages..."
        npm install
        if ($LASTEXITCODE -ne 0) {
            throw "npm install failed"
        }
    }

    Write-Info "Checking PostgreSQL..."
    & powershell -ExecutionPolicy Bypass -File "./scripts/start-postgres.ps1"

    Write-Info "Initializing database..."
    node "./scripts/init-db.js"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Database initialization failed - but continuing..."
    }

    Write-Info "Cleaning .NET projects..."
    dotnet clean "src/Greenhouse.sln"

    Write-Info "Restoring .NET dependencies..."
    dotnet restore "src/Greenhouse.sln"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error ".NET restore failed, but continuing..."
    }
    
    Write-Info "Building .NET project..."
    dotnet build "src/Server/Server.csproj" --verbosity quiet --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error ".NET build failed, trying to clean and rebuild..."
        
        # Попытка очистки и повторной сборки
        dotnet clean "src/Server/Server.csproj"
        dotnet build "src/Server/Server.csproj" --verbosity normal
        
        if ($LASTEXITCODE -ne 0) {
            throw ".NET build failed after cleanup"
        }
    }
    
    # Запуск .NET сервера
    Write-Info "Starting .NET server..."
    $dotnetProcess = Start-Process -PassThru -NoNewWindow -FilePath "dotnet" `
        -ArgumentList "run --project src/Server/Server.csproj --urls http://localhost:5000"

    if (-not $dotnetProcess) {
        throw "Failed to start .NET server"
    }

    Write-Info "Waiting for .NET server to start..."
    Start-Sleep -Seconds 7
    
    # Проверка доступности порта 3000 перед запуском Node.js
    Write-Info "Final check for port 3000 availability..."
    $maxRetries = 1
    $retryCount = 0
    $nodePort = "3000"
    
    while ($retryCount -lt $maxRetries -and -not (Test-PortAvailable 3000)) {
        Write-Info "Port 3000 is still in use, retrying... ($($retryCount + 1)/$maxRetries)"
        Stop-PortProcess 3000
        Start-Sleep -Seconds 3
        $retryCount++
    }
    
    if (-not (Test-PortAvailable 3000)) {
        Write-Error "Port 3000 is still in use after $maxRetries attempts. Using alternative port..."
        # Используем альтернативный порт
        $nodePort = "3001"
        Write-Info "Node.js will use alternative port: $nodePort"
    }
    
    # Запуск Node.js сервера
    Write-Info "Starting Node.js server on port $nodePort..."
    $nodeProcess = Start-Process -PassThru -NoNewWindow -FilePath "node" `
        -ArgumentList "src/server.js", "--port", $nodePort
    
    if (-not $nodeProcess) {
        throw "Failed to start Node.js server"
    }
    
    Write-Success "Both servers are running!"
    Write-Info ".NET Server: http://localhost:5000"
    Write-Info "Node.js Server: http://localhost:$nodePort"
    Write-Info ".NET Server PID: $($dotnetProcess.Id)"
    Write-Info "Node.js Server PID: $($nodeProcess.Id)"
    Write-Host "`nPress Ctrl+C to stop servers" -ForegroundColor Yellow

    do {
        Start-Sleep -Seconds 1
    } while ($true)

} catch {
    Write-Host "`n[ERROR] Error during startup: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($dotnetProcess) { 
        try { Stop-Process -Id $dotnetProcess.Id -Force -ErrorAction SilentlyContinue } catch {} 
    }
    if ($nodeProcess) { 
        try { Stop-Process -Id $nodeProcess.Id -Force -ErrorAction SilentlyContinue } catch {} 
    }
    
    exit 1
}