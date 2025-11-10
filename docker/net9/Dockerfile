FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src
COPY . .

RUN dotnet publish "src/LINQBenchmark.Runner.net9/LINQBenchmark.Runner.net9.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "LINQBenchmark.Runner.net9.dll"]