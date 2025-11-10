FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

WORKDIR /src/src/LINQBenchmark.Runner
RUN dotnet publish -c Release -f net8.0 -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .

ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.dll"]