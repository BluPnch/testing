using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitTests.Builders;
using UnitTests.ObjectMother;

namespace UnitTests.TestServices
{
    [AllureFeature("Client Service")]
    [AllureStory("Client Management Operations")]
    public class TestClientService
    {
        private readonly Mock<IClientRepository> _mockRepository;
        private readonly Mock<IPlantRepository> _mockPlantRepository;
        private readonly Mock<IJournalRecordRepository> _mockJournalRepository;
        private readonly Mock<IAuthUserRepository> _mockAuthRepository;
        private readonly ClientService _service;
        private readonly ClientValidator _clientValidator;
        private readonly Mock<ILogger<ClientService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public TestClientService()
        {
            _mockRepository = new Mock<IClientRepository>();
            _mockPlantRepository = new Mock<IPlantRepository>();
            _mockJournalRepository = new Mock<IJournalRecordRepository>();
            _mockAuthRepository = new Mock<IAuthUserRepository>();
            _clientValidator = new ClientValidator();
            _mockLogger = new Mock<ILogger<ClientService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new ClientService(
                _mockRepository.Object,
                _mockPlantRepository.Object,
                _mockJournalRepository.Object,
                _mockAuthRepository.Object,
                _clientValidator,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        #region CreateClient Tests
        [Fact]
        [AllureName("Create client - should create client when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task CreateClientAsync_ShouldCreateClient_WhenValidData()
        {
            await AllureApi.Step("Setup valid client data", () => {
                var client = new ClientBuilder()
                    .WithCompanyName("Test-Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
            });

            await AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.CreateClientAsync(client))
                    .ReturnsAsync(client);
            });

            var result = await AllureApi.Step("Execute CreateClientAsync", 
                async () => await _service.CreateClientAsync(client));

            await AllureApi.Step("Verify client created successfully", () => {
                Assert.Equal(client.Id, result.Id);
                Assert.Equal(client.CompanyName, result.CompanyName);
                Assert.Equal(client.PhoneNumber, result.PhoneNumber);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.CreateClientAsync(client), Times.Once);
            });

            await AllureApi.Step("Verify logging occurred", () => {
                _mockLogger.Verify(
                    x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.AtLeastOnce);
            });
        }
        
        [Fact]
        [AllureName("Create client - should throw exception when client is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateClientAsync_ShouldThrowArgumentNullException_WhenClientIsNull()
        {
            await AllureApi.Step("Attempt to create null client", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateClientAsync(null!));
                
                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("client", exception.ParamName);
                    Assert.Contains("client", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreateClientAsync(It.IsAny<Client>()), Times.Never);
            });

            await AllureApi.Step("Verify error logging occurred", () => {
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => true),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.AtLeastOnce);
            });
        }

        [Fact]
        [AllureName("Create client - should throw exception when invalid company name")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateClientAsync_ShouldThrowArgumentException_WhenInvalidCompanyName()
        {
            await AllureApi.Step("Setup client with invalid company name", () => {
                var invalidClient = new ClientBuilder()
                    .WithCompanyName("") // Empty company name - invalid
                    .WithPhoneNumber("1234567890")
                    .Build();
            });

            await AllureApi.Step("Attempt to create client with invalid data", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.CreateClientAsync(invalidClient));
                
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("company", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [AllureName("Create client - should throw exception when invalid phone number")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateClientAsync_ShouldThrowArgumentException_WhenInvalidPhoneNumber()
        {
            await AllureApi.Step("Setup client with invalid phone number", () => {
                var invalidClient = new ClientBuilder()
                    .WithCompanyName("Valid Company")
                    .WithPhoneNumber("invalid-phone") // Invalid phone format
                    .Build();
            });

            await AllureApi.Step("Attempt to create client with invalid phone", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.CreateClientAsync(invalidClient));
                
                await AllureApi.Step("Verify phone validation error", () => {
                    Assert.Contains("phone", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region GetClient Tests
        [Fact]
        [AllureName("Get client by ID - should return client when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenExists()
        {
            var clientId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository response", () => {
                var expectedClient = ClientObjectMother.CreateDefaultClient();
                _mockRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                    .ReturnsAsync(expectedClient);
            });

            var result = await AllureApi.Step($"Execute GetClientByIdAsync for ID: {clientId}", 
                async () => await _service.GetClientByIdAsync(clientId));

            await AllureApi.Step("Verify client returned", () => {
                Assert.NotNull(result);
                Assert.Equal(clientId, result.Id);
                Assert.Equal("Test-Company", result.CompanyName);
                Assert.Equal("1234567890", result.PhoneNumber);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get client by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetClientByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(repo => repo.GetClientByIdAsync(nonExistentId))
                    .ReturnsAsync((Client?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent client with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.GetClientByIdAsync(nonExistentId));
            });
        }
        #endregion

        #region GetAllClients Tests
        [Fact]
        [AllureName("Get all clients - should return all clients")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            await AllureApi.Step("Setup mock repository with multiple clients", () => {
                var clients = new List<Client>
                {
                    ClientObjectMother.CreateDefaultClient(),
                    ClientObjectMother.CreateDefaultClient()
                };
                
                _mockRepository.Setup(repo => repo.GetAllClientsAsync())
                    .ReturnsAsync(clients);
            });

            var result = await AllureApi.Step("Execute GetAllClientsAsync", 
                async () => await _service.GetAllClientsAsync());

            await AllureApi.Step("Verify clients returned", () => {
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetAllClientsAsync(), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get all clients - should return empty list when no clients")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllClientsAsync_ShouldReturnEmptyList_WhenNoClients()
        {
            await AllureApi.Step("Setup mock repository with empty list", () => {
                _mockRepository.Setup(repo => repo.GetAllClientsAsync())
                    .ReturnsAsync(new List<Client>());
            });

            var result = await AllureApi.Step("Execute GetAllClientsAsync", 
                async () => await _service.GetAllClientsAsync());

            await AllureApi.Step("Verify empty list returned", () => {
                Assert.NotNull(result);
                Assert.Empty(result);
            });
        }
        #endregion

        #region GetPlantsByClientId Tests
        [Fact]
        [AllureName("Get plants by client ID - should return plants when client exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantsByClientIdAsync_ShouldReturnPlants_WhenClientExists()
        {
            var clientId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository responses", () => {
                var client = ClientObjectMother.CreateDefaultClient();
                _mockRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                    .ReturnsAsync(client);

                var plants = new List<Plant>
                {
                    PlantObjectMother.CreateDefaultPlant(),
                    PlantObjectMother.CreateDefaultPlant()
                };
                
                _mockRepository.Setup(repo => repo.GetPlantsByClientIdAsync(clientId))
                    .ReturnsAsync(plants);
            });

            var result = await AllureApi.Step($"Execute GetPlantsByClientIdAsync for client ID: {clientId}", 
                async () => await _service.GetPlantsByClientIdAsync(clientId));

            await AllureApi.Step("Verify plants returned", () => {
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
            });

            await AllureApi.Step("Verify repository methods called", () => {
                _mockRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
                _mockRepository.Verify(repo => repo.GetPlantsByClientIdAsync(clientId), Times.Once);
            });
        }
        #endregion
    }
}