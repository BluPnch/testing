Write-Host "=== STARTING POSTGRESQL ===" -ForegroundColor Cyan

# Проверяем, запущен ли PostgreSQL
$postgresProcess = Get-Process -Name "postgres" -ErrorAction SilentlyContinue

if ($postgresProcess) {
    Write-Host "PostgreSQL is already running (PID: $($postgresProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "PostgreSQL is not running. Please start it manually:" -ForegroundColor Yellow
    Write-Host "1. Open 'Services' (services.msc)" -ForegroundColor White
    Write-Host "2. Find 'postgresql-x64-16' or similar" -ForegroundColor White
    Write-Host "3. Start the service" -ForegroundColor White
    Write-Host "Or install PostgreSQL if not installed: https://www.postgresql.org/download/" -ForegroundColor White
}