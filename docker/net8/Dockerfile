FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner/LINQBenchmark.Runner.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.dll"]