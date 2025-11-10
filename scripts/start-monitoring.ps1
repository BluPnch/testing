# Запускаем всю инфраструктуру мониторинга
Write-Host "Starting monitoring infrastructure..." -ForegroundColor Green

docker-compose down --remove-orphans

# Запускаем только мониторинг (без benchmark)
docker-compose up -d prometheus grafana cadvisor

Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

Write-Host "Monitoring services started:" -ForegroundColor Green
Write-Host "Grafana: http://localhost:3001 (admin/admin)" -ForegroundColor Cyan
Write-Host "Prometheus: http://localhost:9090" -ForegroundColor Cyan
Write-Host "cAdvisor: http://localhost:8089" -ForegroundColor Cyan

Write-Host "`nNow you can run benchmarks and watch metrics in real-time!" -ForegroundColor Yellow