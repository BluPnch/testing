FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src
COPY . .

WORKDIR /src/src/LINQBenchmark.Runner
RUN dotnet publish -c Release -f net9.0 -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.dll"]