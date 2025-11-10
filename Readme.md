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