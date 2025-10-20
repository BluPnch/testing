using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using Domain.Models.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UnitTests.Builders;
using UnitTests.MotherObjects;

namespace UnitTests.TestServices
{
    public class TestPlantService
    {
        private readonly Mock<IPlantRepository> _mockRepository;
        private readonly Mock<IClientRepository> _mockClientRepository;
        private readonly PlantService _service;
        private readonly PlantValidator _plantValidator;
        private readonly Mock<ILogger<PlantService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public TestPlantService()
        {
            _mockRepository = new Mock<IPlantRepository>();
            _mockClientRepository = new Mock<IClientRepository>();
            _plantValidator = new PlantValidator();
            _mockLogger = new Mock<ILogger<PlantService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new PlantService(
                _mockRepository.Object,
                _mockClientRepository.Object,
                _plantValidator,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }
        
        private Plant CreateValidPlant(Guid? customId = null)
        {
            return new PlantBuilder().Build();
        }

        #region CreatePlant Tests
        [Fact]
        public async Task CreatePlantAsync_ShouldCreatePlant_WhenValidData()
        {
            
            var plant = PlantMotherObject.CreateDefaultPlant();
            var clientId = plant.ClientId;
            var client = new Client(clientId, "Test Company", "1234567890");

            _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                .ReturnsAsync(client);
            _mockRepository.Setup(repo => repo.CreatePlantAsync(plant, clientId))
                .ReturnsAsync(plant);

            
            var result = await _service.CreatePlantAsync(plant, clientId);

            // Assert
            Assert.Equal(plant.Id, result.Id);
            Assert.Equal(plant.Specie, result.Specie);
            Assert.Equal(plant.Family, result.Family);
            _mockClientRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
            _mockRepository.Verify(repo => repo.CreatePlantAsync(plant, clientId), Times.Once);
        }

        
        [Fact]
        public async Task CreatePlantAsync_ShouldThrowArgumentNullException_WhenPlantIsNull()
        {
            
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreatePlantAsync(null!, Guid.NewGuid()));
            
            Assert.Equal("plant", ex.ParamName);
            _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreatePlantAsync_ShouldThrowArgumentException_WhenClientIdIsEmpty()
        {
            
            var plant = PlantMotherObject.CreateDefaultPlant();
            
            
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePlantAsync(plant, Guid.Empty));
            
            Assert.Equal("clientId", ex.ParamName);
            _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CreatePlantAsync_ShouldThrowArgumentException_WhenClientNotFound()
        {
            
            var plant = PlantMotherObject.CreateDefaultPlant();
            var clientId = Guid.NewGuid();
            _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                               .ReturnsAsync((Client?)null);

            
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreatePlantAsync(plant, clientId));
            
            Assert.Contains("Клиент не найден", ex.Message);
            _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>(), It.IsAny<Guid>()), Times.Never);
        }
        #endregion

        #region UpdatePlant Tests
        [Fact]
        public async Task UpdatePlantAsync_ShouldUpdatePlant_WhenValidData()
        {
            
            var plant = PlantMotherObject.CreateDefaultPlant();

            _mockRepository.Setup(repo => repo.UpdatePlantAsync(plant))
                .ReturnsAsync(plant);

            
            await _service.UpdatePlantAsync(plant);

            // Assert
            _mockRepository.Verify(repo => repo.UpdatePlantAsync(plant), Times.Once);
        }
        
        [Fact]
        public async Task UpdatePlantAsync_ShouldThrowArgumentNullException_WhenPlantIsNull()
        {
            
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.UpdatePlantAsync(null!));
            
            Assert.Equal("plant", ex.ParamName);
            _mockRepository.Verify(repo => repo.UpdatePlantAsync(It.IsAny<Plant>()), Times.Never);
        }
        #endregion
    }
}