FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine

WORKDIR /src
COPY . .

# Проверим что скопировалось
RUN echo "=== Listing files ==="
RUN find . -name "*.csproj" -type f
RUN echo "=== LINQBenchmark.Runner ==="
RUN ls -la src/LINQBenchmark.Runner/ || echo "Directory not found"
RUN echo "=== LINQBenchmark ==="
RUN ls -la src/LINQBenchmark/ || echo "Directory not found"

# Проверим структуру src
RUN echo "=== src structure ==="
RUN ls -la src/