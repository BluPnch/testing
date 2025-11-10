FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner/LINQBenchmark.Runner.csproj" \
    -c Release \
    -f net9.0 \
    -o /app/publish \
    -p:TargetFramework=net9.0

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.dll"]