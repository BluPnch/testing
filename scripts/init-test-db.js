require('dotenv').config();
const { Pool } = require('pg');

function getPoolConfig(database = 'postgres') {
    return {
        host: process.env.DB_HOST || 'localhost',
        port: process.env.DB_PORT || 5433, 
        database: database,
        user: process.env.DB_USER || 'postgres',
        password: process.env.DB_PASSWORD || '1',
        connectionTimeoutMillis: 5000,
        idleTimeoutMillis: 30000
    };
}

async function initTestDatabase() {
    const dbName = 'test_db';
    console.log(`Инициализация тестовой базы данных: ${dbName}`);

    try {
        const pool = new Pool(getPoolConfig());

        // Проверяем подключение
        await pool.query('SELECT 1');
        console.log('Connected to PostgreSQL successfully');

        // Удаляем базу если существует
        await pool.query(`DROP DATABASE IF EXISTS ${dbName}`);

        // Создаем новую базу
        await pool.query(`CREATE DATABASE ${dbName}`);
        console.log(`База данных ${dbName} создана успешно`);

        await pool.end();
        console.log('Тестовая база данных готова к использованию');

    } catch (error) {
        console.error('Ошибка инициализации тестовой базы:', error);
        process.exit(1);
    }
}

if (require.main === module) {
    initTestDatabase();
}

module.exports = { initTestDatabase };