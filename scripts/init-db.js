require('dotenv').config();
const { Pool } = require('pg');

function getPoolConfig(database = 'postgres') {
    return {
        host: process.env.DB_HOST || 'localhost',
        port: process.env.DB_PORT || 5433,
        database: database,
        user: process.env.DB_USER || 'postgres',
        password: process.env.DB_PASSWORD || 1,
        connectionTimeoutMillis: 5000,
        idleTimeoutMillis: 30000
    };
}

async function initDatabase() {
    const dbName = process.env.DB_NAME || 'GreenhouseContext';
    console.log(`Initializing database: ${dbName}`);

    try {
        const pool = new Pool(getPoolConfig());

        await pool.query('SELECT 1');
        console.log('Connected to PostgreSQL successfully');

        const result = await pool.query(
            "SELECT 1 FROM pg_database WHERE datname = $1",
            [dbName]
        );

        if (result.rowCount === 0) {
            await pool.query(`CREATE DATABASE "${dbName}"`);
            console.log(`Database ${dbName} created successfully`);
        } else {
            console.log(`Database ${dbName} already exists`);
        }

        await pool.end();

        await initDatabaseStructure(dbName);

    } catch (error) {
        console.error('Database initialization error:', error.message);
        console.log('Please check:');
        console.log('1. PostgreSQL is running');
        console.log('2. Connection settings in .env file');
        console.log('3. Username/password are correct');
        console.log('Default connection: postgres:1@localhost:5433');
        process.exit(1);
    }
}

async function initDatabaseStructure(dbName) {
    const pool = new Pool(getPoolConfig(dbName));

    try {
        console.log(`Initializing database structure for ${dbName}...`);

        await pool.query('SELECT 1');

        console.log(`Database ${dbName} is ready for Entity Framework migrations`);

    } catch (error) {
        console.error('Error initializing database structure:', error.message);
        throw error;
    } finally {
        await pool.end();
    }
}

if (require.main === module) {
    initDatabase();
}

module.exports = { initDatabase };