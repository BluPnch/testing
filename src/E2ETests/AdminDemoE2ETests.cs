using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Testing;
using Server;

namespace E2ETests
{
    public class AdminRealE2ETests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Конфигурация для тестов
        private const string AdminEmail = "admin@gh.com";
        private const string AdminPassword = "admin123";

        public AdminRealE2ETests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost"),
                HandleCookies = true,
                AllowAutoRedirect = true
            });
            
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("User-Agent", "E2E-Test-Suite/1.0");
            
            // Увеличиваем таймаут для CI
            _client.Timeout = TimeSpan.FromSeconds(120);
        }

        [Fact(Timeout = 120000)]
        public async Task Administrator_Assigns_Client_As_Employee_Should_Succeed()
        {
            string authToken = string.Empty;
            string existingClientId = string.Empty;
            string assignedEmployeeId = string.Empty;

            try
            {
                // === PHASE 1: АУТЕНТИФИКАЦИЯ АДМИНИСТРАТОРА ===
                await LogTestStep("1. Аутентификация администратора");
                authToken = await AuthenticateAsAdmin();
                authToken.Should().NotBeNullOrEmpty("Токен аутентификации должен быть получен");

                // === PHASE 2: ПОЛУЧЕНИЕ СУЩЕСТВУЮЩЕГО КЛИЕНТА ===
                await LogTestStep("2. Получение существующего клиента из базы данных");
                var allClients = await GetClientsList();
                allClients.Should().NotBeNull("Список клиентов не должен быть null");
                allClients.Should().NotBeEmpty("В базе данных должен быть хотя бы один клиент");
                
                // Берем первого клиента из списка
                var existingClient = allClients.First();
                existingClientId = existingClient.Id.ToString();
                existingClientId.Should().NotBeNullOrEmpty("ID существующего клиента не должен быть пустым");

                await LogTestStep($"3. Используем клиента: {existingClient.CompanyName} (ID: {existingClientId})");

                // === PHASE 3: НАЗНАЧЕНИЕ КЛИЕНТА СОТРУДНИКОМ ===
                await LogTestStep("4. Назначение клиента на роль сотрудника");
                assignedEmployeeId = await AssignClientAsEmployee(existingClientId);
                assignedEmployeeId.Should().NotBeNullOrEmpty("ID назначенного сотрудника не должен быть пустым");

                // === PHASE 4: ПРОВЕРКА РЕЗУЛЬТАТА ===
                await LogTestStep("5. Проверка, что клиент стал сотрудником");
                var employees = await GetEmployeesList();
                employees.Should().Contain(e => e.Id.ToString() == existingClientId, // Теперь ID сотрудника = ID клиента
                    "Назначенный сотрудник должен отображаться в списке сотрудников");

                var employeeDetails = await GetEmployeeDetails(existingClientId); // Используем existingClientId
                employeeDetails.Should().NotBeNull("Детали сотрудника не должны быть null");
                employeeDetails.ClientId.Should().Be(existingClientId, "Сотрудник должен быть привязан к клиенту");

                await LogTestStep("✅ СЦЕНАРИЙ УСПЕШНО ЗАВЕРШЕН: Клиент назначен сотрудником");

            }
            catch (Exception ex)
            {
                await LogTestStep($"❌ ТЕСТ ПРЕРВАН С ОШИБКОЙ: {ex.Message}");
                
                // Попытка очистки тестовых данных в случае ошибки
                if (!string.IsNullOrEmpty(assignedEmployeeId))
                {
                    try { await DeleteEmployee(assignedEmployeeId); } catch { }
                }
                
                throw;
            }
            finally
            {
                // Финальная очистка (удаляем только созданного сотрудника, клиент остается)
                if (!string.IsNullOrEmpty(assignedEmployeeId))
                {
                    try { await DeleteEmployee(assignedEmployeeId); } catch { }
                }
            }
        }

        private async Task<string> AuthenticateAsAdmin()
        {
            var loginRequest = new 
            { 
                username = AdminEmail, 
                password = AdminPassword 
            };
            
            var loginPayload = JsonSerializer.Serialize(loginRequest, JsonOptions);
            using var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/v1/auth/login", loginContent);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Authentication failed: {response.StatusCode} - {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponse>(responseContent, JsonOptions);
            
            // Устанавливаем токен для последующих запросов
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
            
            return result.Token;
        }

        private async Task<List<ClientItem>> GetClientsList()
        {
            var response = await _client.GetAsync("/api/v1/clients?page=1&pageSize=50");
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get clients failed: {response.StatusCode} - {errorContent}");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<ClientItem>>(content, JsonOptions);
            
            return result ?? new List<ClientItem>();
        }

        private async Task<string> AssignClientAsEmployee(string clientId)
        {
            var updateRoleRequest = new
            {
                newRole = "Employee" // Используем строковое значение роли
            };
    
            var payload = JsonSerializer.Serialize(updateRoleRequest, JsonOptions);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
    
            var response = await _client.PatchAsync($"/api/v1/clients/{clientId}/role", content);
    
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Assign client as employee failed: {response.StatusCode} - {errorContent}");
            }
    
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthUserResponse>(responseContent, JsonOptions);
    
            return result.Id.ToString();
        }


        private async Task<List<EmployeeItem>> GetEmployeesList()
        {
            var response = await _client.GetAsync("/api/v1/employees?page=1&pageSize=50");
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get employees failed: {response.StatusCode} - {errorContent}");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<EmployeeItem>>(content, JsonOptions);
            
            return result ?? new List<EmployeeItem>();
        }

        private async Task<EmployeeItem> GetEmployeeDetails(string employeeId)
        {
            var response = await _client.GetAsync($"/api/v1/employees/{employeeId}");
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get employee details failed: {response.StatusCode} - {errorContent}");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmployeeItem>(content, JsonOptions);
        }

        private async Task<bool> DeleteEmployee(string employeeId)
        {
            // Возвращаем роль обратно к Client
            var updateRoleRequest = new
            {
                newRole = "Client"
            };
    
            var payload = JsonSerializer.Serialize(updateRoleRequest, JsonOptions);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
    
            var response = await _client.PatchAsync($"/api/v1/clients/{employeeId}/role", content);
            return response.StatusCode == HttpStatusCode.OK;
        }

        private async Task LogTestStep(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{timestamp}] {message}");
            await SaveTestLog(message);
        }

        private async Task SaveTestLog(string message)
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "e2e-logs");
            Directory.CreateDirectory(logDir);
            
            var logFile = Path.Combine(logDir, $"employee-assignment-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            await File.AppendAllTextAsync(logFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
        }

        public void Dispose()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }
    }

    // Response models
    public class LoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class ClientItem
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class EmployeeItem
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
    }
    
    public class AuthUserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}