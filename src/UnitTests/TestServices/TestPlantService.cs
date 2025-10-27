using Allure.Xunit.Attributes;
using Allure.Net.Commons;
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
    [AllureFeature("Plant Service")]
    [AllureStory("Plant Management Operations")]
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
        [AllureName("Create plant - should create plant when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreatePlantAsync_ShouldCreatePlant_WhenValidData()
        {
            await AllureApi.Step("Setup valid plant and client data", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                var clientId = plant.ClientId;
                var client = new Client(clientId, "Test Company", "1234567890");
            });

            await AllureApi.Step("Setup mock repository responses", () => {
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                    .ReturnsAsync(client);
                _mockRepository.Setup(repo => repo.CreatePlantAsync(plant, clientId))
                    .ReturnsAsync(plant);
            });

            var result = await AllureApi.Step($"Execute CreatePlantAsync for client ID: {clientId}", 
                async () => await _service.CreatePlantAsync(plant, clientId));

            await AllureApi.Step("Verify plant created successfully", () => {
                Assert.Equal(plant.Id, result.Id);
                Assert.Equal(plant.Specie, result.Specie);
                Assert.Equal(plant.Family, result.Family);
                Assert.Equal("Rose", result.Specie);
                Assert.Equal("Rosaceae", result.Family);
            });

            await AllureApi.Step("Verify repository methods called", () => {
                _mockClientRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
                _mockRepository.Verify(repo => repo.CreatePlantAsync(plant, clientId), Times.Once);
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
        [AllureName("Create plant - should throw exception when plant is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldThrowArgumentNullException_WhenPlantIsNull()
        {
            await AllureApi.Step("Attempt to create null plant", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreatePlantAsync(null!, Guid.NewGuid()));
                
                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("plant", exception.ParamName);
                    Assert.Contains("plant", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>(), It.IsAny<Guid>()), Times.Never);
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
        [AllureName("Create plant - should throw exception when client ID is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldThrowArgumentException_WhenClientIdIsEmpty()
        {
            await AllureApi.Step("Setup valid plant with empty client ID", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
            });

            await AllureApi.Step("Attempt to create plant with empty client ID", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.CreatePlantAsync(plant, Guid.Empty));
                
                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("clientId", exception.ParamName);
                    Assert.Contains("clientId", exception.Message);
                    Assert.Contains("empty", exception.Message.ToLower());
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>(), It.IsAny<Guid>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Create plant - should throw exception when client not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldThrowArgumentException_WhenClientNotFound()
        {
            var clientId = Guid.NewGuid();
            
            await AllureApi.Step("Setup plant with non-existent client", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
            });

            await AllureApi.Step("Setup mock client repository to return null", () => {
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                                   .ReturnsAsync((Client?)null);
            });

            await AllureApi.Step($"Attempt to create plant for non-existent client ID: {clientId}", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.CreatePlantAsync(plant, clientId));
                
                await AllureApi.Step("Verify exception message", () => {
                    Assert.Contains("Клиент не найден", exception.Message);
                    Assert.Contains(clientId.ToString(), exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>(), It.IsAny<Guid>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Create plant - should validate plant species when species is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldValidateSpecies_WhenSpeciesIsEmpty()
        {
            await AllureApi.Step("Setup plant with empty species", () => {
                var invalidPlant = new PlantBuilder()
                    .WithSpecie("")
                    .WithFamily("ValidFamily")
                    .Build();
            });

            await AllureApi.Step("Setup mock client repository", () => {
                var client = new Client(Guid.NewGuid(), "Test Company", "1234567890");
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(It.IsAny<Guid>()))
                                   .ReturnsAsync(client);
            });

            await AllureApi.Step("Attempt to create plant with empty species", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreatePlantAsync(invalidPlant, Guid.NewGuid()));
                
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("species", exception.Message.ToLower());
                    Assert.Contains("required", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [AllureName("Create plant - should validate plant family when family is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldValidateFamily_WhenFamilyIsEmpty()
        {
            await AllureApi.Step("Setup plant with empty family", () => {
                var invalidPlant = new PlantBuilder()
                    .WithSpecie("ValidSpecies")
                    .WithFamily("")
                    .Build();
            });

            await AllureApi.Step("Setup mock client repository", () => {
                var client = new Client(Guid.NewGuid(), "Test Company", "1234567890");
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(It.IsAny<Guid>()))
                                   .ReturnsAsync(client);
            });

            await AllureApi.Step("Attempt to create plant with empty family", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreatePlantAsync(invalidPlant, Guid.NewGuid()));
                
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("family", exception.Message.ToLower());
                    Assert.Contains("required", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region UpdatePlant Tests
        [Fact]
        [AllureName("Update plant - should update plant when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdatePlantAsync_ShouldUpdatePlant_WhenValidData()
        {
            await AllureApi.Step("Setup valid plant for update", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
            });

            await AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.UpdatePlantAsync(plant))
                    .ReturnsAsync(plant);
            });

            await AllureApi.Step("Execute UpdatePlantAsync", 
                async () => await _service.UpdatePlantAsync(plant));

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.UpdatePlantAsync(plant), Times.Once);
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
        [AllureName("Update plant - should throw exception when plant is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdatePlantAsync_ShouldThrowArgumentNullException_WhenPlantIsNull()
        {
            await AllureApi.Step("Attempt to update null plant", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.UpdatePlantAsync(null!));
                
                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("plant", exception.ParamName);
                    Assert.Contains("plant", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdatePlantAsync(It.IsAny<Plant>()), Times.Never);
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
        [AllureName("Update plant - should validate plant data during update")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdatePlantAsync_ShouldThrowArgumentException_WhenInvalidData()
        {
            await AllureApi.Step("Setup plant with invalid data for update", () => {
                var invalidPlant = new PlantBuilder()
                    .WithSpecie("")
                    .WithFamily("")
                    .Build();
            });

            await AllureApi.Step("Attempt to update plant with invalid data", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.UpdatePlantAsync(invalidPlant));
                
                await AllureApi.Step("Verify validation errors", () => {
                    Assert.Contains("species", exception.Message.ToLower());
                    Assert.Contains("family", exception.Message.ToLower());
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdatePlantAsync(It.IsAny<Plant>()), Times.Never);
            });
        }
        #endregion

        #region GetPlant Tests
        [Fact]
        [AllureName("Get plant by ID - should return plant when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetPlantByIdAsync_ShouldReturnPlant_WhenExists()
        {
            var plantId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository response", () => {
                var expectedPlant = PlantMotherObject.CreateDefaultPlant();
                _mockRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                    .ReturnsAsync(expectedPlant);
            });

            var result = await AllureApi.Step($"Execute GetPlantByIdAsync for ID: {plantId}", 
                async () => await _service.GetPlantByIdAsync(plantId));

            await AllureApi.Step("Verify plant returned", () => {
                Assert.NotNull(result);
                Assert.Equal(plantId, result.Id);
                Assert.Equal("Rose", result.Specie);
                Assert.Equal("Rosaceae", result.Family);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetPlantByIdAsync(plantId), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get plant by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(repo => repo.GetPlantByIdAsync(nonExistentId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent plant with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.GetPlantByIdAsync(nonExistentId));
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetPlantByIdAsync(nonExistentId), Times.Once);
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
                var client = new Client(clientId, "Test Company", "1234567890");
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                    .ReturnsAsync(client);

                var plants = new List<Plant>
                {
                    PlantMotherObject.CreateDefaultPlant(),
                    PlantMotherObject.CreateDefaultPlant()
                };
                
                _mockRepository.Setup(repo => repo.GetPlantsByClientIdAsync(clientId))
                    .ReturnsAsync(plants);
            });

            var result = await AllureApi.Step($"Execute GetPlantsByClientIdAsync for client ID: {clientId}", 
                async () => await _service.GetPlantsByClientIdAsync(clientId));

            await AllureApi.Step("Verify plants returned", () => {
                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
                Assert.All(result, p => Assert.Equal(clientId, p.ClientId));
            });

            await AllureApi.Step("Verify repository methods called", () => {
                _mockClientRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
                _mockRepository.Verify(repo => repo.GetPlantsByClientIdAsync(clientId), Times.Once);
            });
        }
        #endregion
    }
}