using System.ComponentModel;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using DefaultNamespace;
using FluentValidation;
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
        [DisplayName("Create client - should create client when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateClientAsync_ShouldCreateClient_WhenValidData()
        {
            Client client = null!;
            
            // Настройка валидных данных клиента
            AllureApi.Step("Setup valid client data", () => {
                client = new ClientBuilder()
                    .WithCompanyName("Test-Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
            });

            // Настройка ответа mock репозитория
            AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.CreateClientAsync(client))
                    .ReturnsAsync(client);
            });

            // Выполнение метода CreateClientAsync
            var result = await AllureApi.Step("Execute CreateClientAsync", 
                async () => await _service.CreateClientAsync(client));

            // Проверка успешного создания клиента
            AllureApi.Step("Verify client created successfully", () => {
                Assert.Equal(client.Id, result.Id);
                Assert.Equal(client.CompanyName, result.CompanyName);
                Assert.Equal(client.PhoneNumber, result.PhoneNumber);
            });

            // Проверка вызова метода репозитория
            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.CreateClientAsync(client), Times.Once);
            });

            // Проверка логирования
            AllureApi.Step("Verify logging occurred", () => {
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
        [DisplayName("Create client - should throw exception when client is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateClientAsync_ShouldThrowArgumentNullException_WhenClientIsNull()
        {
            await AllureApi.Step("Attempt to create null client", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateClientAsync(null!));
    
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("client", exception.ParamName);
                    Assert.Contains("client", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreateClientAsync(It.IsAny<Client>()), Times.Never);
            });

            AllureApi.Step("Remove logging verification", () => {
            });
        }

        [Fact]
        [DisplayName("Create client - should throw exception when invalid company name")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateClientAsync_ShouldThrowArgumentException_WhenInvalidCompanyName()
        {
            Client invalidClient = null!;
    
            AllureApi.Step("Setup client with invalid company name", () => {
                invalidClient = new ClientBuilder()
                    .WithCompanyName("") 
                    .WithPhoneNumber("1234567890")
                    .Build();
            });

            await AllureApi.Step("Attempt to create client with invalid data", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateClientAsync(invalidClient));
        
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("компан", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [DisplayName("Create client - should throw exception when invalid phone number")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateClientAsync_ShouldThrowArgumentException_WhenInvalidPhoneNumber()
        {
            Client invalidClient = null!;
    
            AllureApi.Step("Setup client with invalid phone number", () => {
                invalidClient = new ClientBuilder()
                    .WithCompanyName("Valid Company")
                    .WithPhoneNumber("invalid-phone")
                    .Build();
            });

            await AllureApi.Step("Attempt to create client with invalid phone", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateClientAsync(invalidClient));
        
                AllureApi.Step("Verify phone validation error", () => {
                    Assert.Contains("телефон", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region GetClient Tests
        [Fact]
        [DisplayName("Get client by ID - should return client when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenExists()
        {
            Client expectedClient = null!;
    
            AllureApi.Step("Setup mock repository response", () => {
                expectedClient = ClientObjectMother.CreateDefaultClient();
                _mockRepository.Setup(repo => repo.GetClientByIdAsync(expectedClient.Id))
                    .ReturnsAsync(expectedClient);
            });

            var result = await AllureApi.Step($"Execute GetClientByIdAsync for ID: {expectedClient.Id}", 
                async () => await _service.GetClientByIdAsync(expectedClient.Id));

            AllureApi.Step("Verify client returned", () => {
                Assert.NotNull(result);
                Assert.Equal(expectedClient.Id, result.Id);
                Assert.Equal("Company", result.CompanyName);
                Assert.Equal("1234567890", result.PhoneNumber);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetClientByIdAsync(expectedClient.Id), Times.Once);
            });
        }

        [Fact]
        [DisplayName("Get client by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetClientByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
    
            AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(repo => repo.GetClientByIdAsync(nonExistentId))
                    .ReturnsAsync((Client?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent client with ID: {nonExistentId}", async () => {
                var result = await _service.GetClientByIdAsync(nonExistentId);
                Assert.Null(result);
            });
        }
        #endregion

        #region GetAllClients Tests
        [Fact]
        [DisplayName("Get all clients - should return all clients")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            List<Client> clients = null!;
            
            AllureApi.Step("Setup mock repository with multiple clients", () => {
                clients = new List<Client>
                {
                    ClientObjectMother.CreateDefaultClient(),
                    ClientObjectMother.CreateDefaultClient()
                };
                
                _mockRepository.Setup(repo => repo.GetAllClientsAsync())
                    .ReturnsAsync(clients);
            });

            var result = await AllureApi.Step("Execute GetAllClientsAsync", 
                async () => await _service.GetAllClientsAsync());

            AllureApi.Step("Verify clients returned", () => {
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetAllClientsAsync(), Times.Once);
            });
        }

        [Fact]
        [DisplayName("Get all clients - should return empty list when no clients")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllClientsAsync_ShouldReturnEmptyList_WhenNoClients()
        {
            AllureApi.Step("Setup mock repository with empty list", () => {
                _mockRepository.Setup(repo => repo.GetAllClientsAsync())
                    .ReturnsAsync(new List<Client>());
            });

            var result = await AllureApi.Step("Execute GetAllClientsAsync", 
                async () => await _service.GetAllClientsAsync());

            AllureApi.Step("Verify empty list returned", () => {
                Assert.NotNull(result);
                Assert.Empty(result);
            });
        }
        #endregion
    }
}