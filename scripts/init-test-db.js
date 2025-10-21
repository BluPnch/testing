require('dotenv').config();
const { Pool } = require('pg');

function getPoolConfig(database = 'postgres') {
    return {
        host: process.env.DB_HOST || 'postgres',
        port: process.env.DB_PORT || 5433, 
        database: database,
        user: process.env.DB_USER || 'postgres',
        password: process.env.DB_PASSWORD || '1',
        connectionTimeoutMillis: 5000,
        idleTimeoutMillis: 30000
    };
}

async function initTestDatabase() {
    const dbName = process.env.DB_NAME || 'GreenhouseContext';
    console.log(`Initializing test database: ${dbName} on ${process.env.DB_HOST || 'postgres'}:${process.env.DB_PORT || 5433}`);

    try {
        const pool = new Pool(getPoolConfig());

        // Проверяем подключение
        await pool.query('SELECT 1');
        console.log('Connected to PostgreSQL successfully');

        // Проверяем подключение
        await pool.query('SELECT 1');
        console.log('✅ Connected to PostgreSQL successfully');

        // Пересоздаем базу
        try {
            // Завершаем активные соединения
            await pool.query(`
                SELECT pg_terminate_backend(pid) 
                FROM pg_stat_activity 
                WHERE datname = $1 AND pid <> pg_backend_pid()
            `, [dbName]);
        } catch (e) {
            // Игнорируем ошибки завершения соединений
        }

        await pool.query(`DROP DATABASE IF EXISTS "${dbName}"`);
        await pool.query(`CREATE DATABASE "${dbName}" ENCODING 'UTF8'`);
        console.log(`✅ Database ${dbName} recreated successfully`);

        await pool.end();
        console.log('✅ Test database ready');

    } catch (error) {
        console.error('❌ Database initialization error:', error.message);
        console.log('Connection details:', {
            host: process.env.DB_HOST || 'postgres',
            port: process.env.DB_PORT || 5433,
            user: process.env.DB_USER || 'postgres',
            database: 'postgres'
        });
        process.exit(1);
    }
}

if (require.main === module) {
    initTestDatabase();
}

module.exports = { initTestDatabase };