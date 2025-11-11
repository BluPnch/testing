# Перезапустить Docker
wsl --shutdown
net stop com.docker.service
net start com.docker.service


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

# Пересобрать контейнеры с выводом логов
docker-compose build --no-cache --progress=plain


# Запуск всей системы с мониторингом
.\scripts\run-benchmarks.ps1 -WithMonitoring $true
# Проверка статуса контейнеров
.\scripts\check-health.ps1
# Запуск только мониторинга
.\scripts\start-monitoring.ps1
# Запуск только бэнчмарка
.\scripts\run-benchmarks.ps1 -Target net8 -WithMonitoring $false


# ! Остановить все!
docker-compose down

.\scripts\start-monitoring.ps1
start http://localhost:3001
[//]: # (Другое окно)
.\scripts\run-benchmarks.ps1 -Target net8 -WithMonitoring $false

# Запустить всё
docker-compose up -d


PS C:\sem7\testing> docker-compose ps
NAME         IMAGE                             COMMAND                   SERVICE          CREATED          STATUS                    PORTS
cadvisor     gcr.io/cadvisor/cadvisor:latest   "/usr/bin/cadvisor -…"    cadvisor         45 seconds ago   Up 44 seconds (healthy)   0.0.0.0:8089->8080/tcp
grafana      grafana/grafana:latest            "/run.sh"                 grafana          45 seconds ago   Up 40 seconds             0.0.0.0:3001->3000/tcp
linq-net8    bench/linq-net8:latest            "/bin/sh -c 'sh -c \"…"   benchmark-net8   45 seconds ago   Up 44 seconds             0.0.0.0:8081->8081/tcp
linq-net9    bench/linq-net9:latest            "dotnet LINQBenchmar…"    benchmark-net9   45 seconds ago   Up 44 seconds             0.0.0.0:8082->8082/tcp
prometheus   prom/prometheus:latest            "/bin/prometheus --c…"    prometheus       45 seconds ago   Up 41 seconds             0.0.0.0:9090->9090/tcp





# Память процесса
container_memory_usage_bytes{container=~"linq-net.*"}
# Аллокации памяти (через GC)
rate(container_memory_failures_total{container=~"linq-net.*"}[5m])
# Размер heap
container_memory_working_set_bytes{container=~"linq-net.*"}

# Количество сборок GC по поколениям
rate(container_memory_failures_total{container=~"linq-net.*", scope="container"}[5m])


# Разница в памяти
container_memory_usage_bytes{container="linq-net8"} - container_memory_usage_bytes{container="linq-net9"}
# Разница в CPU (производительность)
rate(container_cpu_user_seconds_total{container="linq-net8"}[5m]) / rate(container_cpu_user_seconds_total{container="linq-net9"}[5m])
# Разница в GC сборках (аллокации)
rate(container_memory_failures_total{container="linq-net8"}[5m]) - rate(container_memory_failures_total{container="linq-net9"}[5m])


# Первый
.\scripts\run-benchmarks.ps1
# Все последующие
.\scripts\run-benchmarks.ps1 -Fast $true