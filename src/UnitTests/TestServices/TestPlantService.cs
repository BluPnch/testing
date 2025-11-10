using System.ComponentModel;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using Domain.Models.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UnitTests.Builders;
using UnitTests.MotherObjects;
using UnitTests.ObjectMother;

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
        [DisplayName("Create plant - should create plant when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreatePlantAsync_ShouldCreatePlant_WhenValidData()
        {
            Plant plant = null!;
            Guid clientId = Guid.Empty;
            Client client = null!;
            
            AllureApi.Step("Setup valid plant and client data", () => {
                plant = PlantMotherObject.CreateDefaultPlant();
                clientId = plant.ClientId;
                client = new Client(clientId, "Test Company", "1234567890");
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
                    .ReturnsAsync(client);
                _mockRepository.Setup(repo => repo.CreatePlantAsync(plant))
                    .ReturnsAsync(plant);
            });

            var result = await AllureApi.Step($"Execute CreatePlantAsync for client ID: {clientId}", 
                async () => await _service.CreatePlantAsync(plant));

            AllureApi.Step("Verify plant created successfully", () => {
                Assert.Equal(plant.Id, result.Id);
                Assert.Equal(plant.Specie, result.Specie);
                Assert.Equal(plant.Family, result.Family);
                Assert.Equal("Rose", result.Specie);
                Assert.Equal("Rosaceae", result.Family);
            });

            AllureApi.Step("Verify repository methods called", () => {
                _mockClientRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
                _mockRepository.Verify(repo => repo.CreatePlantAsync(plant), Times.Once);
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
        [DisplayName("Create plant - should throw exception when plant is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldThrowArgumentNullException_WhenPlantIsNull()
        {
            await AllureApi.Step("Attempt to create null plant", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreatePlantAsync(null!));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("plant", exception.ParamName);
                    Assert.Contains("plant", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreatePlantAsync(It.IsAny<Plant>()), Times.Never);
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
        [DisplayName("Create plant - should validate plant species when species is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldValidateSpecies_WhenSpeciesIsEmpty()
        {
            Plant invalidPlant = null!;
    
            AllureApi.Step("Setup plant with empty species", () => {
                invalidPlant = new PlantBuilder()
                    .WithSpecie("")
                    .WithFamily("ValidFamily")
                    .Build();
            });

            AllureApi.Step("Setup mock client repository", () => {
                var client = new Client(Guid.NewGuid(), "Test Company", "1234567890");
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(client);
            });

            await AllureApi.Step("Attempt to create plant with empty species", async () => {
                var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                    _service.CreatePlantAsync(invalidPlant));
        
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("failed to create plant", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [DisplayName("Create plant - should validate plant family when family is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldValidateFamily_WhenFamilyIsEmpty()
        {
            Plant invalidPlant = null!;
    
            AllureApi.Step("Setup plant with empty family", () => {
                invalidPlant = new PlantBuilder()
                    .WithSpecie("ValidSpecies")
                    .WithFamily("")
                    .Build();
            });

            AllureApi.Step("Setup mock client repository", () => {
                var client = new Client(Guid.NewGuid(), "Test Company", "1234567890");
                _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(client);
            });

            await AllureApi.Step("Attempt to create plant with empty family", async () => {
                var exception = await Assert.ThrowsAsync<ApplicationException>(() =>
                    _service.CreatePlantAsync(invalidPlant));
        
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("failed to create plant", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region UpdatePlant Tests
        [Fact]
        [DisplayName("Update plant - should update plant when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdatePlantAsync_ShouldUpdatePlant_WhenValidData()
        {
            Plant plant = null!;
            
            AllureApi.Step("Setup valid plant for update", () => {
                plant = PlantMotherObject.CreateDefaultPlant();
            });

            AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.UpdatePlantAsync(plant))
                    .ReturnsAsync(plant);
            });

            await AllureApi.Step("Execute UpdatePlantAsync", 
                async () => await _service.UpdatePlantAsync(plant));

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.UpdatePlantAsync(plant), Times.Once);
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
        [DisplayName("Update plant - should throw exception when plant is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdatePlantAsync_ShouldThrowArgumentNullException_WhenPlantIsNull()
        {
            await AllureApi.Step("Attempt to update null plant", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.UpdatePlantAsync(null!));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("plant", exception.ParamName);
                    Assert.Contains("plant", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdatePlantAsync(It.IsAny<Plant>()), Times.Never);
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
        [DisplayName("Update plant - should validate plant data during update")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdatePlantAsync_ShouldThrowArgumentException_WhenInvalidData()
        {
            Plant invalidPlant = null!;
    
            AllureApi.Step("Setup plant with invalid data for update", () => {
                invalidPlant = new PlantBuilder()
                    .WithSpecie("")
                    .WithFamily("")
                    .Build();
            });

            await AllureApi.Step("Attempt to update plant with invalid data", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                    _service.UpdatePlantAsync(invalidPlant));
        
                AllureApi.Step("Verify validation errors", () => {
                    Assert.Contains("вид", exception.Message.ToLower());
                    Assert.Contains("семейство", exception.Message.ToLower());
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdatePlantAsync(It.IsAny<Plant>()), Times.Never);
            });
        }
        #endregion

        #region GetPlant Tests
        [Fact]
        [DisplayName("Get plant by ID - should return plant when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetPlantByIdAsync_ShouldReturnPlant_WhenExists()
        {
            Plant expectedPlant = null!;
    
            AllureApi.Step("Setup mock repository response", () => {
                expectedPlant = PlantMotherObject.CreateDefaultPlant();
                _mockRepository.Setup(repo => repo.GetPlantByIdAsync(expectedPlant.Id))
                    .ReturnsAsync(expectedPlant);
            });

            var result = await AllureApi.Step($"Execute GetPlantByIdAsync for ID: {expectedPlant.Id}", 
                async () => await _service.GetPlantByIdAsync(expectedPlant.Id));

            AllureApi.Step("Verify plant returned", () => {
                Assert.NotNull(result);
                Assert.Equal(expectedPlant.Id, result.Id);
                Assert.Equal("Rose", result.Specie);
                Assert.Equal("Rosaceae", result.Family);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetPlantByIdAsync(expectedPlant.Id), Times.Once);
            });
        }

        [Fact]
        [DisplayName("Get plant by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
    
            AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(repo => repo.GetPlantByIdAsync(nonExistentId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent plant with ID: {nonExistentId}", async () => {
                var result = await _service.GetPlantByIdAsync(nonExistentId);
                Assert.Null(result);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetPlantByIdAsync(nonExistentId), Times.Once);
            });
        }
        #endregion

        // #region GetPlantsByClientId Tests
        // [Fact]
        // [DisplayName("Get plants by client ID - should return plants when client exists")]
        // [AllureOwner("Development Team")]
        // [AllureSeverity(SeverityLevel.normal)]
        // public async Task GetPlantsByClientIdAsync_ShouldReturnPlants_WhenClientExists()
        // {
        //     var clientId = Guid.NewGuid();
        //     Client client = null!;
        //     List<Plant> plants = null!;
        //     
        //     AllureApi.Step("Setup mock repository responses", () => {
        //         client = new Client(clientId, "Test Company", "1234567890");
        //         _mockClientRepository.Setup(repo => repo.GetClientByIdAsync(clientId))
        //             .ReturnsAsync(client);
        //
        //         plants = new List<Plant>
        //         {
        //             PlantMotherObject.CreateDefaultPlant(),
        //             PlantMotherObject.CreateDefaultPlant()
        //         };
        //         
        //         _mockRepository.Setup(repo => repo.GetPlantsByClientIdAsync(clientId))
        //             .ReturnsAsync(plants);
        //     });
        //
        //     var result = await AllureApi.Step($"Execute GetPlantsByClientIdAsync for client ID: {clientId}", 
        //         async () => await _service.GetPlantsByClientIdAsync(clientId));
        //
        //     AllureApi.Step("Verify plants returned", () => {
        //         Assert.NotNull(result);
        //         Assert.Equal(2, result.Count());
        //         Assert.All(result, p => Assert.Equal(clientId, p.ClientId));
        //     });
        //
        //     AllureApi.Step("Verify repository methods called", () => {
        //         _mockClientRepository.Verify(repo => repo.GetClientByIdAsync(clientId), Times.Once);
        //         _mockRepository.Verify(repo => repo.GetPlantsByClientIdAsync(clientId), Times.Once);
        //     });
        // }
        // #endregion
    }
}