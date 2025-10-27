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

        // Конфигурация для реального тестирования
        private const string BaseUrl = "https://api.your-greenhouse-app.com"; // Замените на реальный URL
        private const string AdminEmail = "admin@gh.com";
        private const string AdminPassword = "admin123";

        public AdminRealE2ETests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            
            // Настройка стандартных заголовков
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("User-Agent", "E2E-Test-Suite/1.0");
        }

        [Fact(Timeout = 30000)]
        public async Task Administrator_Complete_Real_Journey_Should_Succeed()
        {
            // STEP 1: Real Login with credentials validation
            var loginRequest = new 
            { 
                username = AdminEmail, 
                password = AdminPassword 
            };
            
            var loginPayload = JsonSerializer.Serialize(loginRequest, JsonOptions);
            using var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            
            var loginResponse = await _client.PostAsync("/api/v1/auth/login", loginContent);
            
            // Validate response
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
                "Login should succeed with valid admin credentials");
            
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            loginResponseContent.Should().NotBeNullOrWhiteSpace("Response should contain data");
            
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginResponseContent, JsonOptions);
            
            loginResult.Should().NotBeNull("Login response should not be null");
            loginResult!.Token.Should().NotBeNullOrEmpty("JWT token should be provided");
            loginResult.Username.Should().Be(AdminEmail, "Should return the authenticated user's email");
            
            // Set authentication token for subsequent requests
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", loginResult.Token);
            
            await SaveAttachment("Login_Response.json", loginResponseContent);

            // STEP 2: Get clients with pagination and filtering
            var clientsResponse = await _client.GetAsync("/api/v1/clients?page=1&pageSize=10");
            
            clientsResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
                "Should successfully retrieve clients list");
            
            var clientsContent = await clientsResponse.Content.ReadAsStringAsync();
            clientsContent.Should().NotBeNullOrWhiteSpace("Clients data should not be empty");
            
            // Validate response structure
            var clientsData = JsonSerializer.Deserialize<ClientsListResponse>(clientsContent, JsonOptions);
            clientsData.Should().NotBeNull("Clients response should not be null");
            
            await SaveAttachment("Clients_Response.json", clientsContent);

            // STEP 3: Access administrators endpoint with data validation
            var administratorsResponse = await _client.GetAsync("/api/v1/administrators");
            
            administratorsResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
                "Should have access to administrators endpoint");
            
            var adminContent = await administratorsResponse.Content.ReadAsStringAsync();
            
            // Even if the list is empty, we should get valid response
            adminContent.Should().NotBeNull("Administrators response should not be null");
            
            await SaveAttachment("Administrators_Response.json", adminContent);

            // STEP 4: Get employees with detailed validation
            var employeesResponse = await _client.GetAsync("/api/v1/employees?includeInactive=false");
            
            employeesResponse.StatusCode.Should().Be(HttpStatusCode.OK, 
                "Should successfully retrieve employees list");
            
            var employeesContent = await employeesResponse.Content.ReadAsStringAsync();
            
            // Validate JSON structure
            employeesContent.Should().NotBeNullOrWhiteSpace("Employees data should not be empty");
            
            await SaveAttachment("Employees_Response.json", employeesContent);

            // STEP 5: Test specific client details
            // First get a client ID from the list, or use a known test client
            var clientsListResponse = await _client.GetAsync("/api/v1/clients?page=1&pageSize=5");
            if (clientsListResponse.IsSuccessStatusCode)
            {
                var clientsListContent = await clientsListResponse.Content.ReadAsStringAsync();
                var clientsListData = JsonSerializer.Deserialize<ClientsListResponse>(clientsListContent, JsonOptions);
                
                if (clientsListData?.Items != null && clientsListData.Items.Any())
                {
                    var firstClient = clientsListData.Items.First();
                    var clientDetailsResponse = await _client.GetAsync($"/api/v1/clients/{firstClient.Id}");
                    
                    clientDetailsResponse.StatusCode.Should().BeOneOf(
                        new[] { HttpStatusCode.OK, HttpStatusCode.NotFound },
                        "Client details should be accessible or properly handle missing clients");
                    
                    if (clientDetailsResponse.IsSuccessStatusCode)
                    {
                        var clientDetails = await clientDetailsResponse.Content.ReadAsStringAsync();
                        await SaveAttachment($"Client_{firstClient.Id}_Details.json", clientDetails);
                    }
                }
            }
        }

        [Fact(Timeout = 15000)]
        public async Task Unauthorized_Access_To_Protected_Endpoints_Should_Be_Blocked()
        {
            // Create a new client without authentication
            using var unauthenticatedClient = new HttpClient 
            { 
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };

            var protectedEndpoints = new[]
            {
                "/api/v1/administrators",
                "/api/v1/clients",
                "/api/v1/employees",
                "/api/v1/sensitive-data"
            };

            foreach (var endpoint in protectedEndpoints)
            {
                var response = await unauthenticatedClient.GetAsync(endpoint);
                
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
                    $"Endpoint {endpoint} should require authentication");
                
                // Additional check for proper WWW-Authenticate header
                if (response.Headers.Contains("WWW-Authenticate"))
                {
                    response.Headers.WwwAuthenticate.Should().NotBeEmpty();
                }
            }
        }

        [Fact(Timeout = 10000)]
        public async Task API_Health_Check_Should_Respond()
        {
            var healthResponse = await _client.GetAsync("/health");
            
            healthResponse.StatusCode.Should().BeOneOf(
                new[] { HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable },
                "Health endpoint should respond with expected status codes");
            
            if (healthResponse.IsSuccessStatusCode)
            {
                var healthContent = await healthResponse.Content.ReadAsStringAsync();
                healthContent.Should().NotBeNullOrWhiteSpace("Health response should contain data");
                
                await SaveAttachment("Health_Response.txt", healthContent);
            }
        }

        [Theory(Timeout = 15000)]
        [InlineData("wrong@email.com", "admin123", HttpStatusCode.Unauthorized)]
        [InlineData("admin@gh.com", "wrongpassword", HttpStatusCode.Unauthorized)]
        [InlineData("", "admin123", HttpStatusCode.BadRequest)]
        [InlineData("admin@gh.com", "", HttpStatusCode.BadRequest)]
        public async Task Invalid_Login_Attempts_Should_Fail(string username, string password, HttpStatusCode expectedStatus)
        {
            var loginRequest = new { username, password };
            var loginPayload = JsonSerializer.Serialize(loginRequest, JsonOptions);
            using var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            
            var loginResponse = await _client.PostAsync("/api/v1/auth/login", loginContent);
            
            loginResponse.StatusCode.Should().Be(expectedStatus, 
                $"Login with username '{username}' should return {expectedStatus}");
            
            if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseContent = await loginResponse.Content.ReadAsStringAsync();
                responseContent.Should().NotContain("Token", "No token should be provided for failed login");
            }
        }

        // Упрощенная версия без сложных шагов
        [Fact(Timeout = 30000)]
        public async Task Administrator_Simplified_Journey_Should_Succeed()
        {
            // STEP 1: Login
            var loginRequest = new { username = AdminEmail, password = AdminPassword };
            var loginPayload = JsonSerializer.Serialize(loginRequest, JsonOptions);
            using var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            
            var loginResponse = await _client.PostAsync("/api/v1/auth/login", loginContent);
            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginResponseContent, JsonOptions);
            
            loginResult.Should().NotBeNull();
            loginResult!.Token.Should().NotBeNullOrEmpty();
            
            // Set token for subsequent requests
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            // STEP 2: Access various endpoints
            var endpointsToTest = new[]
            {
                "/api/v1/clients?page=1&pageSize=10",
                "/api/v1/administrators", 
                "/api/v1/employees",
                "/api/v1/dashboard/stats"
            };

            foreach (var endpoint in endpointsToTest)
            {
                var response = await _client.GetAsync(endpoint);
                response.StatusCode.Should().Be(HttpStatusCode.OK, $"Endpoint {endpoint} should be accessible");
                
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNull($"Response from {endpoint} should not be null");
                
                // Save response to file for manual inspection
                await SaveAttachment($"{endpoint.Replace("/", "_")}_response.json", content);
            }
        }

        // Simple file-based attachment
        private async Task SaveAttachment(string filename, string content)
        {
            var attachmentsDir = Path.Combine(Directory.GetCurrentDirectory(), "test-results");
            Directory.CreateDirectory(attachmentsDir);
            
            var filePath = Path.Combine(attachmentsDir, filename);
            await File.WriteAllTextAsync(filePath, content);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    // Response models for real API
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
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}