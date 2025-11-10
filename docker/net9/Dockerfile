FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner.net9/LINQBenchmark.Runner.net9.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine
WORKDIR /app

# Копируем скомпилированное приложение
COPY --from=builder /app/publish .

# Копируем исходные файлы для BenchmarkDotNet
COPY --from=builder /src ./src

ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.net9.dll"]