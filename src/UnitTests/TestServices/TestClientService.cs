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
        public async Task CreateClientAsync_ShouldCreateClient_WhenValidData()
        {
            
            var client = new ClientBuilder()
                .WithCompanyName("Test-Company")
                .WithPhoneNumber("1234567890")
                .Build();
            
            _mockRepository.Setup(repo => repo.CreateClientAsync(client))
                .ReturnsAsync(client);

            
            var result = await _service.CreateClientAsync(client);

            // Assert
            Assert.Equal(client.Id, result.Id);
            Assert.Equal(client.CompanyName, result.CompanyName);
            Assert.Equal(client.PhoneNumber, result.PhoneNumber);
            _mockRepository.Verify(repo => repo.CreateClientAsync(client), Times.Once);
        }
        
        [Fact]
        public async Task CreateClientAsync_ShouldThrowArgumentNullException_WhenClientIsNull()
        {
            
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateClientAsync(null!));
            
            Assert.Equal("client", ex.ParamName);
            _mockRepository.Verify(repo => repo.CreateClientAsync(It.IsAny<Client>()), Times.Never);
        }
        #endregion
    }
}