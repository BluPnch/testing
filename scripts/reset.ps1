$env:PGPASSWORD = "1"
& 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -U postgres -p 5433 -c "DROP DATABASE IF EXISTS `"GreenhouseContext`";"
& 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -U postgres -p 5433 -c "CREATE DATABASE `"GreenhouseContext`";" 