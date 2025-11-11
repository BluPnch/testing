FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

WORKDIR /src
COPY . .

# Восстанавливаем и публикуем NET8 проект
RUN dotnet restore "src/ContinuousMetricsService.net8/ContinuousMetricsService.net8.csproj"
RUN dotnet publish "src/ContinuousMetricsService.net8/ContinuousMetricsService.net8.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ContinuousMetricsService.net8.dll"]