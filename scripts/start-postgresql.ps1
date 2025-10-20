Write-Host "=== STARTING POSTGRESQL ===" -ForegroundColor Cyan

# Проверяем, запущен ли PostgreSQL
try {
    $postgresProcess = Get-Process -Name "postgres" -ErrorAction SilentlyContinue
    if ($postgresProcess) {
        Write-Host "✅ PostgreSQL is already running (PID: $($postgresProcess.Id))" -ForegroundColor Green
        exit 0
    }
} catch {
    # Ignore errors
}

Write-Host "PostgreSQL is not running. Please start it manually:" -ForegroundColor Yellow
Write-Host "1. Open 'Services' (press Win+R and type: services.msc)" -ForegroundColor White
Write-Host "2. Find 'postgresql-x64-*' service" -ForegroundColor White
Write-Host "3. Start the service" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "Or use one of these methods:" -ForegroundColor Yellow
Write-Host "• Start via Start Menu: Search for 'pgAdmin' and start PostgreSQL from there" -ForegroundColor White
Write-Host "• Start via Command Line as Administrator: 'net start postgresql-x64-15'" -ForegroundColor White
Write-Host "• Start via PostgreSQL Stack Builder" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "If PostgreSQL is not installed, download it from: https://www.postgresql.org/download/" -ForegroundColor Yellow