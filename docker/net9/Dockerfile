FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS builder

WORKDIR /src
COPY . .

# Восстанавливаем и публикуем NET9 проект
RUN dotnet restore "src/ContinuousMetricsService.net9/ContinuousMetricsService.net9.csproj"
RUN dotnet publish "src/ContinuousMetricsService.net9/ContinuousMetricsService.net9.csproj" \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ContinuousMetricsService.net9.dll"]