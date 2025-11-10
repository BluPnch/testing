# Перезапустить Docker
wsl --shutdown
net stop com.docker.service
net start com.docker.service

# Остановить текущие контейнеры
docker-compose down
# Пересобрать с исправленными Dockerfile
docker-compose build --no-cache
# Запустить заново
docker-compose up -d
# Проверить логи
docker-compose logs -f benchmark-net8


# Очистить логи контейнера linq-net8
docker-compose logs --tail=0 benchmark-net8
# Или напрямую через docker
docker logs --tail=0 linq-net8



# Остановить и удалить ВСЕ контейнеры
docker-compose down --remove-orphans
# Удалите все образы 
docker system prune -a -f
# Удалить папку results
Remove-Item -Recurse -Force results -ErrorAction SilentlyContinue
# Создать папку results заново
New-Item -ItemType Directory -Path results -Force
->
C:\sem7\testing\scripts\run-benchmarks.ps1
