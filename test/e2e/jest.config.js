module.exports = {
    testEnvironment: 'node',
    reporters: [
        'default',
        ['jest-allure', {
            resultsDir: 'allure-results'
        }]
    ],
    setupFilesAfterEnv: ['jest-allure/dist/setup'],
    testMatch: ['**/*.test.js'],
    collectCoverage: false
};