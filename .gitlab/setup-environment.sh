#!/bin/bash
set -e

echo "Setting up test environment using .env configuration..."

# Используем переменные из .env файла
export DB_HOST=postgres
export DB_PORT=5433
export DB_USER=postgres
export DB_PASSWORD=1
export DB_NAME=GreenhouseContext
export NODE_ENV=development
export ASPNETCORE_ENVIRONMENT=Test

# Ожидание запуска PostgreSQL на порту 5433
echo "Waiting for PostgreSQL to be ready on port 5433..."
for i in {1..30}; do
  if pg_isready -h postgres -p 5433 -U postgres; then
    echo "PostgreSQL is ready on port 5433!"
    break
  fi
  echo "Waiting for PostgreSQL... attempt $i"
  sleep 2
done

# Проверяем, что база данных доступна
echo "Checking database connection..."
psql -h postgres -p 5433 -U postgres -d GreenhouseContext -c "SELECT 1;" || {
  echo "Creating database GreenhouseContext..."
  psql -h postgres -p 5433 -U postgres -c "CREATE DATABASE \"GreenhouseContext\";"
}

# Инициализация тестовой базы данных
echo "Initializing test database using .env configuration..."
node scripts/init-test-db.js

echo "Environment setup completed!"