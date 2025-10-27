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
using UnitTests.MotherObjects;

namespace UnitTests.TestServices
{
    [AllureFeature("Seed Service")]
    [AllureStory("Seed Management Operations")]
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
        [AllureName("Create seed - should throw exception when seed is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldThrowArgumentNullException_WhenSeedIsNull()
        {
            await AllureApi.Step("Attempt to create null seed", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateSeedAsync(null!));
                
                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("seed", exception.ParamName);
                    Assert.Contains("seed", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(It.IsAny<Seed>()), Times.Never);
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
        [AllureName("Create seed - should throw exception when plant not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldThrowKeyNotFoundException_WhenPlantNotFound()
        {
            var plantId = Guid.NewGuid();
            
            await AllureApi.Step("Setup seed with non-existent plant", () => {
                var validSeed = new SeedBuilder()
                    .WithPlantId(plantId)
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository to return null", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step($"Attempt to create seed for non-existent plant ID: {plantId}", async () => {
                var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => _service.CreateSeedAsync(validSeed));
        
                await AllureApi.Step("Verify exception message", () => {
                    Assert.Contains($"Растение с ID {plantId} не найдено", exception.Message);
                    Assert.Contains(plantId.ToString(), exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(It.IsAny<Seed>()), Times.Never);
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
        [AllureName("Create seed - should create seed when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateSeedAsync_ShouldCreateSeed_WhenValidData()
        {
            await AllureApi.Step("Setup valid seed and plant data", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                var seed = new SeedBuilder()
                    .WithPlantId(plant.Id)
                    .WithMaturity("Mature")
                    .WithViability(Domain.Models.Enums.EnumViability.Healthy)
                    .WithLightRequirements(Domain.Models.Enums.EnumLight.Medium)
                    .WithWaterRequirements("Moderate")
                    .WithTemperatureRequirements(25)
                    .Build();
            });

            await AllureApi.Step("Setup mock repository responses", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plant.Id))
                    .ReturnsAsync(plant);
                _mockSeedRepository.Setup(repo => repo.CreateSeedAsync(seed))
                    .ReturnsAsync(seed);
            });

            var result = await AllureApi.Step("Execute CreateSeedAsync", 
                async () => await _service.CreateSeedAsync(seed));

            await AllureApi.Step("Verify seed created successfully", () => {
                Assert.Equal(seed.Id, result.Id);
                Assert.Equal(seed.PlantId, result.PlantId);
                Assert.Equal(seed.Maturity, result.Maturity);
                Assert.Equal(seed.Viability, result.Viability);
                Assert.Equal(seed.LightRequirements, result.LightRequirements);
                Assert.Equal(seed.WaterRequirements, result.WaterRequirements);
                Assert.Equal(seed.TemperatureRequirements, result.TemperatureRequirements);
                Assert.Equal("Mature", result.Maturity);
                Assert.Equal(Domain.Models.Enums.EnumViability.Healthy, result.Viability);
            });

            await AllureApi.Step("Verify repository methods called", () => {
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plant.Id), Times.Once);
                _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(seed), Times.Once);
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
        [AllureName("Create seed - should validate maturity when maturity is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldValidateMaturity_WhenMaturityIsEmpty()
        {
            await AllureApi.Step("Setup seed with empty maturity", () => {
                var invalidSeed = new SeedBuilder()
                    .WithPlantId(Guid.NewGuid())
                    .WithMaturity("")
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(plant);
            });

            await AllureApi.Step("Attempt to create seed with empty maturity", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateSeedAsync(invalidSeed));
                
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("maturity", exception.Message.ToLower());
                    Assert.Contains("required", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [AllureName("Create seed - should validate temperature when temperature is out of range")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldValidateTemperature_WhenTemperatureIsOutOfRange()
        {
            await AllureApi.Step("Setup seed with invalid temperature", () => {
                var invalidSeed = new SeedBuilder()
                    .WithPlantId(Guid.NewGuid())
                    .WithTemperatureRequirements(60) // Too high temperature
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(plant);
            });

            await AllureApi.Step("Attempt to create seed with invalid temperature", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateSeedAsync(invalidSeed));
                
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("temperature", exception.Message.ToLower());
                    Assert.Contains("range", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region UpdateSeedAsync Tests
        [Fact]
        [AllureName("Update seed - should update seed when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdateSeedAsync_ShouldUpdateSeed_WhenValidData()
        {
            await AllureApi.Step("Setup valid seed and plant data for update", () => {
                var seed = SeedMotherObject.CreateDefaultSeed();
                var plant = PlantMotherObject.CreateDefaultPlant();
            });

            await AllureApi.Step("Setup mock repository responses", () => {
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seed.Id))
                    .ReturnsAsync(seed);
                
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(seed.PlantId))
                    .ReturnsAsync(plant);
                
                _mockSeedRepository.Setup(repo => repo.UpdateSeedAsync(seed))
                    .ReturnsAsync(seed);
            });

            await AllureApi.Step($"Execute UpdateSeedAsync for seed ID: {seed.Id}", 
                async () => await _service.UpdateSeedAsync(seed));

            await AllureApi.Step("Verify repository methods called", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedByIdAsync(seed.Id), Times.Once);
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(seed.PlantId), Times.Once);
                _mockSeedRepository.Verify(repo => repo.UpdateSeedAsync(seed), Times.Once);
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
        [AllureName("Update seed - should throw exception when seed not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateSeedAsync_ShouldThrowException_WhenSeedNotFound()
        {
            var seedId = Guid.NewGuid();
            
            await AllureApi.Step("Setup non-existent seed", () => {
                var seed = new SeedBuilder()
                    .WithId(seedId)
                    .Build();
            });

            await AllureApi.Step("Setup mock repository to return null seed", () => {
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seedId))
                    .ReturnsAsync((Seed?)null);
            });

            await AllureApi.Step($"Attempt to update non-existent seed with ID: {seedId}", async () => {
                await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    _service.UpdateSeedAsync(seed));
            });

            await AllureApi.Step("Verify repository methods called appropriately", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedByIdAsync(seedId), Times.Once);
                _mockSeedRepository.Verify(repo => repo.UpdateSeedAsync(It.IsAny<Seed>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Update seed - should throw exception when plant not found during update")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateSeedAsync_ShouldThrowException_WhenPlantNotFound()
        {
            await AllureApi.Step("Setup seed with non-existent plant", () => {
                var seed = SeedMotherObject.CreateDefaultSeed();
            });

            await AllureApi.Step("Setup mock repository responses", () => {
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seed.Id))
                    .ReturnsAsync(seed);
                
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(seed.PlantId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step($"Attempt to update seed with non-existent plant ID: {seed.PlantId}", async () => {
                var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    _service.UpdateSeedAsync(seed));
                
                await AllureApi.Step("Verify exception message", () => {
                    Assert.Contains($"Растение с ID {seed.PlantId} не найдено", exception.Message);
                });
            });
        }
        #endregion

        #region GetSeed Tests
        [Fact]
        [AllureName("Get seed by ID - should return seed when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetSeedByIdAsync_ShouldReturnSeed_WhenExists()
        {
            var seedId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository response", () => {
                var expectedSeed = SeedMotherObject.CreateDefaultSeed();
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seedId))
                    .ReturnsAsync(expectedSeed);
            });

            var result = await AllureApi.Step($"Execute GetSeedByIdAsync for ID: {seedId}", 
                async () => await _service.GetSeedByIdAsync(seedId));

            await AllureApi.Step("Verify seed returned", () => {
                Assert.NotNull(result);
                Assert.Equal(seedId, result.Id);
                Assert.Equal("Mature", result.Maturity);
                Assert.Equal(Domain.Models.Enums.EnumViability.Healthy, result.Viability);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedByIdAsync(seedId), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get seeds by maturity - should return seeds when maturity matches")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetSeedsByMaturityAsync_ShouldReturnSeeds_WhenMaturityMatches()
        {
            var maturity = "Mature";
            
            await AllureApi.Step("Setup mock repository response", () => {
                var seeds = new List<Seed>
                {
                    SeedMotherObject.CreateDefaultSeed(),
                    SeedMotherObject.CreateDefaultSeed()
                };
                
                _mockSeedRepository.Setup(repo => repo.GetSeedsByMaturityAsync(maturity))
                    .ReturnsAsync(seeds);
            });

            var result = await AllureApi.Step($"Execute GetSeedsByMaturityAsync for maturity: {maturity}", 
                async () => await _service.GetSeedsByMaturityAsync(maturity));

            await AllureApi.Step("Verify seeds returned", () => {
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, s => Assert.Equal(maturity, s.Maturity));
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedsByMaturityAsync(maturity), Times.Once);
            });
        }
        #endregion
    }
}