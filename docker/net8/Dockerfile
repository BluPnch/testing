FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

# Собираем NET8 версию
RUN dotnet publish "src/LINQBenchmark.Runner.net8/LINQBenchmark.Runner.net8.csproj" \
    -c Release \
    -o /app/publish

# Используем SDK образ вместо runtime
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.net8.dll"]