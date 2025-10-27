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

namespace E2ETests
{
    public class AdminRealE2ETests : IDisposable
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private const string BaseUrl = "http://localhost:5097";
        private const string AdminEmail = "admin@gh.com";
        private const string AdminPassword = "admin123";
        private const string TestClientName = "Test Client E2E";
        private const string TestClientEmail = "testclient-e2e@example.com";

        public AdminRealE2ETests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(60)
            };
            
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("User-Agent", "E2E-Test-Suite/1.0");
        }

        [Fact(Timeout = 60000)]
        public async Task Administrator_Assigns_Client_As_Employee_Should_Succeed()
        {
            string authToken = string.Empty;
            string createdClientId = string.Empty;
            string assignedEmployeeId = string.Empty;

            try
            {
                // === PHASE 1: СОЗДАНИЕ КЛИЕНТА ===
                await LogTestStep("1. Создание нового клиента");
                createdClientId = await CreateNewClient();
                createdClientId.Should().NotBeNullOrEmpty("ID созданного клиента не должен быть пустым");

                // === PHASE 2: АУТЕНТИФИКАЦИЯ АДМИНИСТРАТОРА ===
                await LogTestStep("2. Аутентификация администратора");
                authToken = await AuthenticateAsAdmin();
                authToken.Should().NotBeNullOrEmpty("Токен аутентификации должен быть получен");

                // === PHASE 3: ПРОСМОТР КЛИЕНТОВ ===
                await LogTestStep("3. Просмотр списка всех клиентов");
                var allClients = await GetClientsList();
                allClients.Should().NotBeNull("Список клиентов не должен быть null");
                allClients.Should().Contain(c => c.Id == createdClientId, 
                    "Созданный клиент должен отображаться в списке");

                // === PHASE 4: НАЗНАЧЕНИЕ КЛИЕНТА СОТРУДНИКОМ ===
                await LogTestStep("4. Назначение клиента на роль сотрудника");
                assignedEmployeeId = await AssignClientAsEmployee(createdClientId);
                assignedEmployeeId.Should().NotBeNullOrEmpty("ID назначенного сотрудника не должен быть пустым");

                // === PHASE 5: ПРОВЕРКА РЕЗУЛЬТАТА ===
                await LogTestStep("5. Проверка, что клиент стал сотрудником");
                var employees = await GetEmployeesList();
                employees.Should().Contain(e => e.Id == assignedEmployeeId,
                    "Назначенный сотрудник должен отображаться в списке сотрудников");

                var employeeDetails = await GetEmployeeDetails(assignedEmployeeId);
                employeeDetails.Should().NotBeNull("Детали сотрудника не должны быть null");
                employeeDetails.Email.Should().Be(TestClientEmail, "Email сотрудника должен совпадать с email клиента");

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
                if (!string.IsNullOrEmpty(createdClientId))
                {
                    try { await DeleteClient(createdClientId); } catch { }
                }
                
                throw;
            }
            finally
            {
                // Финальная очистка
                if (!string.IsNullOrEmpty(assignedEmployeeId))
                {
                    try { await DeleteEmployee(assignedEmployeeId); } catch { }
                }
                if (!string.IsNullOrEmpty(createdClientId))
                {
                    try { await DeleteClient(createdClientId); } catch { }
                }
            }
        }

        private async Task<string> CreateNewClient()
        {
            var clientRequest = new
            {
                name = TestClientName,
                phone = "+1234567890",
                email = TestClientEmail
            };
            
            var payload = JsonSerializer.Serialize(clientRequest, JsonOptions);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/v1/clients", content);
            
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Create client failed: {response.StatusCode} - {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ClientItem>(responseContent, JsonOptions);
            
            return result.Id;
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
            var result = JsonSerializer.Deserialize<ClientsListResponse>(content, JsonOptions);
            
            return result?.Items ?? new List<ClientItem>();
        }

        private async Task<string> AssignClientAsEmployee(string clientId)
        {
            // Получаем детали клиента
            var clientDetails = await GetClientDetails(clientId);
            
            // Создаем сотрудника на основе данных клиента
            var employeeRequest = new
            {
                firstName = clientDetails.Name, // Используем имя клиента как firstName
                lastName = "Employee", // Добавляем фамилию
                email = clientDetails.Email,
                phone = clientDetails.Phone,
                position = "Assigned from Client",
                clientId = clientId, // Привязываем к самому себе как клиенту
                hireDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
            };
            
            var payload = JsonSerializer.Serialize(employeeRequest, JsonOptions);
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync("/api/v1/employees", content);
            
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Assign client as employee failed: {response.StatusCode} - {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<EmployeeItem>(responseContent, JsonOptions);
            
            return result.Id;
        }

        private async Task<ClientItem> GetClientDetails(string clientId)
        {
            var response = await _client.GetAsync($"/api/v1/clients/{clientId}");
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Get client details failed: {response.StatusCode} - {errorContent}");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ClientItem>(content, JsonOptions);
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
            var result = JsonSerializer.Deserialize<EmployeesListResponse>(content, JsonOptions);
            
            return result?.Items ?? new List<EmployeeItem>();
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
            var response = await _client.DeleteAsync($"/api/v1/employees/{employeeId}");
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
        }

        private async Task<bool> DeleteClient(string clientId)
        {
            var response = await _client.DeleteAsync($"/api/v1/clients/{clientId}");
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent;
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

    public class ClientsListResponse
    {
        public List<ClientItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class ClientItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class EmployeesListResponse
    {
        public List<EmployeeItem> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class EmployeeItem
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
    }
}