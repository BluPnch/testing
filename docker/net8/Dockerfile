FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner.net8/LINQBenchmark.Runner.net8.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
WORKDIR /app
RUN mkdir -p /app/results

# Копируем скомпилированное приложение
COPY --from=builder /app/publish .

# Копируем исходные файлы для BenchmarkDotNet
COPY --from=builder /src ./src

# Добавляем health check для мониторинга
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:80/ || exit 1

# Устанавливаем curl для health check (если нужно)
RUN apk add --no-cache curl

ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.net8.dll"]