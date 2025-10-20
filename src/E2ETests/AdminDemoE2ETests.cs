using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Allure.Net.Commons;
using Xunit;
using FluentAssertions;
using Allure.Xunit.Attributes;

namespace E2ETests
{
    [AllureSuite("Administrator Demo E2E Tests")]
    [AllureFeature("Complete User Journey")]
    public class AdminDemoE2ETests : IClassFixture<GreenhouseWebApplicationFactory>
    {
        private readonly GreenhouseWebApplicationFactory _factory;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AdminDemoE2ETests(GreenhouseWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [AllureStory("Complete Administrator Journey")]
        [AllureSeverity(SeverityLevel.critical)]
        [Fact]
        public async Task Administrator_Complete_Journey_Demo_Should_Succeed()
        {
            var client = _factory.CreateClient();

            // STEP 1: Login
            var loginRequest = new { username = "admin@gh.com", password = "admin123" };
            var loginPayload = JsonSerializer.Serialize(loginRequest);
            using var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);

            loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginResponseContent, JsonOptions);

            loginResult.Should().NotBeNull();
            loginResult!.Token.Should().NotBeNullOrEmpty();

            // Set token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

            // STEP 2: View clients
            var clientsResponse = await client.GetAsync("/api/v1/clients");
            clientsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // STEP 3: Check admin access
            var administratorsResponse = await client.GetAsync("/api/v1/administrators");
            administratorsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var employeesResponse = await client.GetAsync("/api/v1/employees");
            employeesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [AllureStory("Administrator Security Demo")]
        [AllureSeverity(SeverityLevel.critical)]
        [Fact]
        public async Task Unauthorized_Access_Demo_Should_Be_Blocked()
        {
            var client = _factory.CreateClient();

            var endpoints = new[] { "/api/v1/administrators", "/api/v1/clients", "/api/v1/employees" };

            foreach (var endpoint in endpoints)
            {
                var response = await client.GetAsync(endpoint);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }
    }

    public class LoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}