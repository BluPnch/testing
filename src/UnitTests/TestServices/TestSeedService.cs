using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitTests.Builders;
using UnitTests.MotherObjects;

namespace UnitTests.TestServices
{
    public class TestSeedService
    {
        private readonly Mock<ISeedRepository> _mockSeedRepository;
        private readonly Mock<IPlantRepository> _mockPlantRepository;
        private readonly SeedService _service;
        private readonly SeedValidator _seedValidator;
        private readonly Mock<ILogger<SeedService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public TestSeedService()
        {
            _mockSeedRepository = new Mock<ISeedRepository>();
            _mockPlantRepository = new Mock<IPlantRepository>();
            _seedValidator = new SeedValidator();
            _mockLogger = new Mock<ILogger<SeedService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new SeedService(
                _mockSeedRepository.Object,
                _mockPlantRepository.Object,
                _seedValidator,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        #region CreateSeedAsync Tests
        [Fact]
        public async Task CreateSeedAsync_ShouldThrowArgumentNullException_WhenSeedIsNull()
        {
            
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateSeedAsync(null!));
            
            Assert.Equal("seed", ex.ParamName);
            _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(It.IsAny<Seed>()), Times.Never);
        }

        [Fact]
        public async Task CreateSeedAsync_ShouldThrowKeyNotFoundException_WhenPlantNotFound()
        {
            
            var plantId = Guid.NewGuid();
            var validSeed = new SeedBuilder()
                .WithPlantId(plantId)
                .Build();

            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                .ReturnsAsync((Plant?)null);

            
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.CreateSeedAsync(validSeed));
    
            Assert.Contains($"Растение с ID {plantId} не найдено", ex.Message);
            _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(It.IsAny<Seed>()), Times.Never);
        }

        [Fact]
        public async Task CreateSeedAsync_ShouldCreateSeed_WhenValidData()
        {
            
            var plant = PlantMotherObject.CreateDefaultPlant();
            var seed = new SeedBuilder()
                .WithPlantId(plant.Id)
                .Build();

            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plant.Id))
                .ReturnsAsync(plant);
            _mockSeedRepository.Setup(repo => repo.CreateSeedAsync(seed))
                .ReturnsAsync(seed);

            
            var result = await _service.CreateSeedAsync(seed);

            // Assert
            Assert.Equal(seed.Id, result.Id);
            Assert.Equal(seed.PlantId, result.PlantId);
            Assert.Equal(seed.Maturity, result.Maturity);
            Assert.Equal(seed.Viability, result.Viability);
            _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plant.Id), Times.Once);
            _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(seed), Times.Once);
        }
        #endregion

        #region UpdateSeedAsync Tests
        [Fact]
        public async Task UpdateSeedAsync_ShouldUpdateSeed_WhenValidData()
        {
            
            var seed = SeedMotherObject.CreateDefaultSeed();
            var plant = PlantMotherObject.CreateDefaultPlant();

            _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seed.Id))
                .ReturnsAsync(seed);
            
            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(seed.PlantId))
                .ReturnsAsync(plant);
            
            _mockSeedRepository.Setup(repo => repo.UpdateSeedAsync(seed))
                .ReturnsAsync(seed);

            
            await _service.UpdateSeedAsync(seed);

            // Assert
            _mockSeedRepository.Verify(repo => repo.UpdateSeedAsync(seed), Times.Once);
        }
        #endregion
    }
}