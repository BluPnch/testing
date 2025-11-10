FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner/LINQBenchmark.Runner.csproj" \
    -c Release \
    -f net8.0 \
    -o /app/publish \
    -p:TargetFramework=net8.0

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.dll"]