# Simple E2E tests runner script

param(
    [string]$Filter = ""
)

Write-Host "Starting E2E tests..." -ForegroundColor Green

# Check directory
if (-not (Test-Path "src/E2ETests/E2ETests.csproj")) {
    Write-Error "E2E tests not found"
    exit 1
}

# Clean previous results
if (Test-Path "allure-results") {
    Remove-Item -Recurse -Force "allure-results"
}

# Build command
$testCommand = "dotnet test src/E2ETests/E2ETests.csproj --configuration Release --logger trx --results-directory test-results"

if ($Filter -ne "") {
    $testCommand += " --filter `"$Filter`""
}

Write-Host "Command: $testCommand" -ForegroundColor Gray

# Run tests
Invoke-Expression $testCommand
$testResult = $LASTEXITCODE

# Copy Allure results
$sourceDir = "src/E2ETests/bin/Release/net9.0/allure-results"
if (Test-Path $sourceDir) {
    Copy-Item -Path "$sourceDir/*" -Destination "allure-results" -Recurse -Force
    Write-Host "Allure results copied" -ForegroundColor Green
}

# Generate report if Allure is installed
$allureCmd = Get-Command "allure" -ErrorAction SilentlyContinue
if ($allureCmd) {
    if (Test-Path "allure-report") {
        Remove-Item -Recurse -Force "allure-report"
    }
    
    allure generate "allure-results" --clean -o "allure-report"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Allure report generated" -ForegroundColor Green
        Write-Host "To view report: allure open allure-report" -ForegroundColor Cyan
    }
}

exit $testResult