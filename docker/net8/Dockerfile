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

ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.net8.dll"]