# check-db.ps1
Write-Host "🔍 Checking PostgreSQL connection..." -ForegroundColor Yellow

# Попробуем несколько способов проверки

# Способ 1: Простой вызов psql
try {
    $env:PGPASSWORD = "1"
    $result = & psql -h localhost -p 5433 -U postgres -c "SELECT 1;" -q -t 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL is running on port 5433" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL is not accessible" -ForegroundColor Red
        Write-Host "Error: $result" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error checking PostgreSQL: $($_.Exception.Message)" -ForegroundColor Red
}

# Способ 2: Проверка через .NET
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = "Server=localhost,5433;Database=postgres;User Id=postgres;Password=1;"
    $connection.Open()
    $connection.Close()
    Write-Host "✅ .NET can connect to PostgreSQL" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET cannot connect to PostgreSQL" -ForegroundColor Red
}

# Способ 3: Проверка порта
try {
    $tcp = New-Object System.Net.Sockets.TcpClient
    $tcp.Connect("localhost", 5433)
    $tcp.Close()
    Write-Host "✅ Port 5433 is open" -ForegroundColor Green
} catch {
    Write-Host "❌ Port 5433 is not open" -ForegroundColor Red
}