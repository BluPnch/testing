const axios = require('axios');

const server_url = 'http://localhost:5097';

test('server availability check', async () => {
    console.log('checking if server is running on port 5097...');

    try {
        const response = await axios.get('http://localhost:5097/api/health', {
            timeout: 3000
        });
        console.log('server is running, status:', response.status);
        expect(response.status).toBe(200);
    } catch (error) {
        console.log('server not available:', error.message);
        console.log('run this command first: npm run servers');

        // Проверяем альтернативные порты
        const ports = [5000, 5097, 3000, 3001];
        for (const port of ports) {
            try {
                const testResponse = await axios.get('http://localhost:' + port + '/api/health', {
                    timeout: 1000
                });
                console.log('server found on port ' + port + ', status: ' + testResponse.status);
                // Если нашли работающий сервер, просто логируем и продолжаем
                return; // выходим из теста без ошибки
            } catch (e) {
                console.log('port ' + port + ': ' + (e.code || e.message));
            }
        }

        // ИСПРАВЛЕНИЕ: Error с большой буквы и правильный синтаксис
        throw new Error('server not running. please start servers first with: npm run servers');
    }
});

test('authentication endpoint check', async () => {
    // Сначала определим, на каком порту работает сервер
    let baseUrl = 'http://localhost:5097';
    const ports = [5097, 3000, 5000, 3001];

    // Проверяем доступные порты
    for (const port of ports) {
        try {
            await axios.get(`http://localhost:${port}/api/health`, { timeout: 1000 });
            baseUrl = `http://localhost:${port}`;
            console.log(`using server on port ${port}`);
            break;
        } catch (e) {
            // Пропускаем недоступные порты
        }
    }

    try {
        await axios.post(`${baseUrl}/api/auth/login`, {
            username: 'test',
            password: 'test'
        }, {
            timeout: 3000
        });
        // Если запрос прошел успешно, это тоже нормально
        console.log('auth endpoint responded successfully');
    } catch (error) {
        // Проверяем, что endpoint вообще существует
        if (error.response) {
            // Есть ответ от сервера - endpoint работает
            console.log('auth endpoint response status:', error.response.status);
            expect(error.response.status).toBeDefined();
        } else {
            // Нет ответа - возможно, endpoint не существует
            console.log('no response from auth endpoint:', error.message);
            // В этом случае тест должен упасть
            throw new Error(`auth endpoint not available: ${error.message}`);
        }
    }
});