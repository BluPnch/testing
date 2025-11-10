FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner.net8/LINQBenchmark.Runner.net8.csproj" \
    -c Release \
    -o /app/publish \
    --self-contained true \
    -r linux-musl-x64

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["./LINQBenchmark.Runner.net8"]  <!-- Обновить имя файла -->