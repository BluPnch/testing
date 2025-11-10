using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Server;
using Xunit; 

namespace IntegrationTests
{
    public class AuthAndPlantsApiTests : IClassFixture<WebApplicationFactory<Program>> // ← Program из Server
    {
        private readonly WebApplicationFactory<Program> _factory;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthAndPlantsApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_Then_GetPlants_Should_Return200()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act - Login
            var loginPayload = JsonSerializer.Serialize(new { username = "admin@gh.com", password = "admin123" });
            using var content = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            var loginResp = await client.PostAsync("/api/v1/auth/login", content);
            
            // Assert - Login successful
            Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

            var loginJson = await loginResp.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginJson);
            var token = loginDoc.RootElement.GetProperty("token").GetString();

            // Act - Get plants with token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var plantsResp = await client.GetAsync("/api/v1/plants");
            
            // Assert - Plants endpoint accessible
            Assert.True(plantsResp.IsSuccessStatusCode, $"Expected success but got {plantsResp.StatusCode}");
        }

        [Fact]
        public async Task GetPlants_WithoutAuth_Should_Return401()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var plantsResp = await client.GetAsync("/api/v1/plants");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, plantsResp.StatusCode);
        }
    }
}
