Write-Host "=== CLEANING INTEGRATION TESTS ===" -ForegroundColor Cyan

Write-Host "Cleaning IntegrationTests obj/bin folders..." -ForegroundColor Yellow
if (Test-Path "src\IntegrationTests\bin") {
    Remove-Item -Path "src\IntegrationTests\bin" -Recurse -Force
    Write-Host "Removed bin folder" -ForegroundColor Green
}

if (Test-Path "src\IntegrationTests\obj") {
    Remove-Item -Path "src\IntegrationTests\obj" -Recurse -Force
    Write-Host "Removed obj folder" -ForegroundColor Green
}

Write-Host "Restoring dependencies..." -ForegroundColor Yellow
dotnet restore src/IntegrationTests/IntegrationTests.csproj

Write-Host "Integration tests cleaned successfully!" -ForegroundColor Green