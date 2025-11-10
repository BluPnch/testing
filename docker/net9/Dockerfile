FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src
COPY . .

# Собираем NET9 версию
RUN dotnet publish "src/LINQBenchmark.Runner.net9/LINQBenchmark.Runner.net9.csproj" \
    -c Release \
    -o /app/publish

# Используем SDK образ вместо runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.net9.dll"]