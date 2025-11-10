using System.ComponentModel;
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
using UnitTests.ObjectMother;

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
        [DisplayName("Create seed - should throw exception when seed is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldThrowArgumentNullException_WhenSeedIsNull()
        {
            await AllureApi.Step("Attempt to create null seed", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateSeedAsync(null!));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("seed", exception.ParamName);
                    Assert.Contains("seed", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(It.IsAny<Seed>()), Times.Never);
            });

            AllureApi.Step("Verify warning logging occurred", () => {
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("null")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.AtLeastOnce);
            });
        }

        [Fact]
        [DisplayName("Create seed - should throw exception when plant not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldThrowKeyNotFoundException_WhenPlantNotFound()
        {
            var plantId = Guid.NewGuid();
            Seed validSeed = null!;
    
            AllureApi.Step("Setup seed with non-existent plant", () => {
                validSeed = new SeedBuilder()
                    .WithPlantId(plantId)
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository to return null", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step($"Attempt to create seed for non-existent plant ID: {plantId}", async () => {
                var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => _service.CreateSeedAsync(validSeed));

                AllureApi.Step("Verify exception message", () => {
                    Assert.Contains($"Растение с ID {plantId} не найдено", exception.Message);
                    Assert.Contains(plantId.ToString(), exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(It.IsAny<Seed>()), Times.Never);
            });

            AllureApi.Step("Verify warning logging occurred", () => {
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                    Times.AtLeastOnce);
            });
        }

        [Fact]
        [DisplayName("Create seed - should create seed when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateSeedAsync_ShouldCreateSeed_WhenValidData()
        {
            Plant plant = null!;
            Seed seed = null!;
            
            AllureApi.Step("Setup valid seed and plant data", () => {
                plant = PlantMotherObject.CreateDefaultPlant();
                seed = new SeedBuilder()
                    .WithPlantId(plant.Id)
                    .WithMaturity("Mature")
                    .WithViability(EnumViability.Contaminated)
                    .WithLightRequirements(Domain.Models.Enums.EnumLight.Medium)
                    .WithWaterRequirements("Moderate")
                    .WithTemperatureRequirements(25)
                    .Build();
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plant.Id))
                    .ReturnsAsync(plant);
                _mockSeedRepository.Setup(repo => repo.CreateSeedAsync(seed))
                    .ReturnsAsync(seed);
            });

            var result = await AllureApi.Step("Execute CreateSeedAsync", 
                async () => await _service.CreateSeedAsync(seed));

            AllureApi.Step("Verify seed created successfully", () => {
                Assert.Equal(seed.Id, result.Id);
                Assert.Equal(seed.PlantId, result.PlantId);
                Assert.Equal(seed.Maturity, result.Maturity);
                Assert.Equal(seed.Viability, result.Viability);
                Assert.Equal(seed.LightRequirements, result.LightRequirements);
                Assert.Equal(seed.WaterRequirements, result.WaterRequirements);
                Assert.Equal(seed.TemperatureRequirements, result.TemperatureRequirements);
                Assert.Equal("Mature", result.Maturity);
                Assert.Equal(EnumViability.Contaminated, result.Viability);
            });

            AllureApi.Step("Verify repository methods called", () => {
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plant.Id), Times.Once);
                _mockSeedRepository.Verify(repo => repo.CreateSeedAsync(seed), Times.Once);
            });

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
        [DisplayName("Create seed - should validate maturity when maturity is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldValidateMaturity_WhenMaturityIsEmpty()
        {
            Seed invalidSeed = null!;
    
            AllureApi.Step("Setup seed with empty maturity", () => {
                invalidSeed = new SeedBuilder()
                    .WithPlantId(Guid.NewGuid())
                    .WithMaturity("")
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(plant);
            });

            await AllureApi.Step("Attempt to create seed with empty maturity", async () => {
                var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                    _service.CreateSeedAsync(invalidSeed));
        
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("failed to create seed", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [DisplayName("Create seed - should validate temperature when temperature is out of range")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateSeedAsync_ShouldValidateTemperature_WhenTemperatureIsOutOfRange()
        {
            Seed invalidSeed = null!;
    
            AllureApi.Step("Setup seed with invalid temperature", () => {
                invalidSeed = new SeedBuilder()
                    .WithPlantId(Guid.NewGuid())
                    .WithTemperatureRequirements(60)
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(plant);
            });

            await AllureApi.Step("Attempt to create seed with invalid temperature", async () => {
                var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                    _service.CreateSeedAsync(invalidSeed));
        
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("failed to create seed", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region UpdateSeedAsync Tests
        [Fact]
        [DisplayName("Update seed - should update seed when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdateSeedAsync_ShouldUpdateSeed_WhenValidData()
        {
            Seed seed = null!;
            Plant plant = null!;
            
            AllureApi.Step("Setup valid seed and plant data for update", () => {
                seed = SeedMotherObject.CreateDefaultSeed();
                plant = PlantMotherObject.CreateDefaultPlant();
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seed.Id))
                    .ReturnsAsync(seed);
                
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(seed.PlantId))
                    .ReturnsAsync(plant);
                
                _mockSeedRepository.Setup(repo => repo.UpdateSeedAsync(seed))
                    .ReturnsAsync(seed);
            });

            await AllureApi.Step($"Execute UpdateSeedAsync for seed ID: {seed.Id}", 
                async () => await _service.UpdateSeedAsync(seed));

            AllureApi.Step("Verify repository methods called", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedByIdAsync(seed.Id), Times.Once);
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(seed.PlantId), Times.Once);
                _mockSeedRepository.Verify(repo => repo.UpdateSeedAsync(seed), Times.Once);
            });

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
        [DisplayName("Update seed - should throw exception when seed not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateSeedAsync_ShouldThrowException_WhenSeedNotFound()
        {
            var seedId = Guid.NewGuid();
            Seed seed = null!;
            
            AllureApi.Step("Setup non-existent seed", () => {
                seed = new SeedBuilder()
                    .WithId(seedId)
                    .Build();
            });

            AllureApi.Step("Setup mock repository to return null seed", () => {
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seedId))
                    .ReturnsAsync((Seed?)null);
            });

            AllureApi.Step($"Attempt to update non-existent seed with ID: {seedId}", async () => {
                await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    _service.UpdateSeedAsync(seed));
            });

            AllureApi.Step("Verify repository methods called appropriately", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedByIdAsync(seedId), Times.Once);
                _mockSeedRepository.Verify(repo => repo.UpdateSeedAsync(It.IsAny<Seed>()), Times.Never);
            });
        }

        [Fact]
        [DisplayName("Update seed - should throw exception when plant not found during update")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateSeedAsync_ShouldThrowException_WhenPlantNotFound()
        {
            Seed seed = null!;
            
            AllureApi.Step("Setup seed with non-existent plant", () => {
                seed = SeedMotherObject.CreateDefaultSeed();
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(seed.Id))
                    .ReturnsAsync(seed);
                
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(seed.PlantId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step($"Attempt to update seed with non-existent plant ID: {seed.PlantId}", async () => {
                var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                    _service.UpdateSeedAsync(seed));
                
                AllureApi.Step("Verify exception message", () => {
                    Assert.Contains($"Растение с ID {seed.PlantId} не найдено", exception.Message);
                });
            });
        }
        #endregion

        #region GetSeed Tests
        [Fact]
        [DisplayName("Get seed by ID - should return seed when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetSeedByIdAsync_ShouldReturnSeed_WhenExists()
        {
            Seed expectedSeed = null!;
    
            AllureApi.Step("Setup mock repository response", () => {
                expectedSeed = SeedMotherObject.CreateDefaultSeed();
                _mockSeedRepository.Setup(repo => repo.GetSeedByIdAsync(expectedSeed.Id))
                    .ReturnsAsync(expectedSeed);
            });

            var result = await AllureApi.Step($"Execute GetSeedByIdAsync for ID: {expectedSeed.Id}", 
                async () => await _service.GetSeedByIdAsync(expectedSeed.Id));

            AllureApi.Step("Verify seed returned", () => {
                Assert.NotNull(result);
                Assert.Equal(expectedSeed.Id, result.Id);
                Assert.Equal("Mature", result.Maturity);
                Assert.Equal(EnumViability.Contaminated, result.Viability);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedByIdAsync(expectedSeed.Id), Times.Once);
            });
        }

        [Fact]
        [DisplayName("Get seeds by maturity - should return seeds when maturity matches")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetSeedsByMaturityAsync_ShouldReturnSeeds_WhenMaturityMatches()
        {
            var maturity = "Mature";
            List<Seed> seeds = null!;
            
            AllureApi.Step("Setup mock repository response", () => {
                seeds = new List<Seed>
                {
                    SeedMotherObject.CreateDefaultSeed(),
                    SeedMotherObject.CreateDefaultSeed()
                };
                
                _mockSeedRepository.Setup(repo => repo.GetSeedsByMaturityAsync(maturity))
                    .ReturnsAsync(seeds);
            });

            var result = await AllureApi.Step($"Execute GetSeedsByMaturityAsync for maturity: {maturity}", 
                async () => await _service.GetSeedsByMaturityAsync(maturity));

            AllureApi.Step("Verify seeds returned", () => {
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, s => Assert.Equal(maturity, s.Maturity));
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockSeedRepository.Verify(repo => repo.GetSeedsByMaturityAsync(maturity), Times.Once);
            });
        }
        #endregion
    }
}