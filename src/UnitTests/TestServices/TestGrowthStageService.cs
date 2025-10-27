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
    [AllureFeature("Growth Stage Service")]
    [AllureStory("Growth Stage Management Operations")]
    public class TestGrowthStageService
    {
        private readonly Mock<IGrowthStageRepository> _mockRepository;
        private readonly GrowthStageService _service;
        private readonly GrowthStageValidator _growthStageValidator;
        private readonly Mock<ILogger<GrowthStageService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public TestGrowthStageService()
        {
            _mockRepository = new Mock<IGrowthStageRepository>();
            _growthStageValidator = new GrowthStageValidator();
            _mockLogger = new Mock<ILogger<GrowthStageService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new GrowthStageService(
                _mockRepository.Object,
                _growthStageValidator,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        #region CreateGrowthStage Tests
        [Fact]
        [AllureName("Create growth stage - should create growth stage when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateGrowthStageAsync_ShouldCreateGrowthStage_WhenValidData()
        {
            await AllureApi.Step("Setup valid growth stage data", () => {
                var growthStage = GrowthStageMotherObject.CreateDefaultGrowthStage();
            });

            await AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.CreateGrowthStageAsync(growthStage))
                              .ReturnsAsync(growthStage);
            });

            var result = await AllureApi.Step("Execute CreateGrowthStageAsync", 
                async () => await _service.CreateGrowthStageAsync(growthStage));

            await AllureApi.Step("Verify growth stage created successfully", () => {
                Assert.Equal(growthStage.Id, result.Id);
                Assert.Equal(growthStage.Name, result.Name);
                Assert.Equal(growthStage.Description, result.Description);
                Assert.Equal("Germination", result.Name);
                Assert.Equal("Seed germination stage", result.Description);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.CreateGrowthStageAsync(growthStage), Times.Once);
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
        [AllureName("Create growth stage - should throw exception when growth stage is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateGrowthStageAsync_ShouldThrowArgumentNullException_WhenGrowthStageIsNull()
        {
            await AllureApi.Step("Attempt to create null growth stage", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _service.CreateGrowthStageAsync(null));

                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("growthStage", exception.ParamName);
                    Assert.Contains("growthStage", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.CreateGrowthStageAsync(It.IsAny<GrowthStage>()), Times.Never);
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
        [AllureName("Create growth stage - should validate name when name is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateGrowthStageAsync_ShouldValidateName_WhenNameIsEmpty()
        {
            await AllureApi.Step("Setup growth stage with empty name", () => {
                var invalidGrowthStage = new GrowthStageBuilder()
                    .WithName("")
                    .WithDescription("Valid description")
                    .Build();
            });

            await AllureApi.Step("Attempt to create growth stage with empty name", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateGrowthStageAsync(invalidGrowthStage));

                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("name", exception.Message.ToLower());
                    Assert.Contains("required", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [AllureName("Create growth stage - should validate name when name is too long")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateGrowthStageAsync_ShouldValidateName_WhenNameIsTooLong()
        {
            await AllureApi.Step("Setup growth stage with too long name", () => {
                var longName = new string('A', 101); // 101 characters - exceeds limit
                var invalidGrowthStage = new GrowthStageBuilder()
                    .WithName(longName)
                    .WithDescription("Valid description")
                    .Build();
            });

            await AllureApi.Step("Attempt to create growth stage with long name", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateGrowthStageAsync(invalidGrowthStage));

                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("name", exception.Message.ToLower());
                    Assert.Contains("length", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [AllureName("Create growth stage - should validate description when description is too long")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateGrowthStageAsync_ShouldValidateDescription_WhenDescriptionIsTooLong()
        {
            await AllureApi.Step("Setup growth stage with too long description", () => {
                var longDescription = new string('D', 1001); // 1001 characters - exceeds limit
                var invalidGrowthStage = new GrowthStageBuilder()
                    .WithName("Valid Name")
                    .WithDescription(longDescription)
                    .Build();
            });

            await AllureApi.Step("Attempt to create growth stage with long description", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateGrowthStageAsync(invalidGrowthStage));

                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("description", exception.Message.ToLower());
                    Assert.Contains("length", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region GetGrowthStage Tests
        [Fact]
        [AllureName("Get growth stage by ID - should return growth stage when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetGrowthStageByIdAsync_ShouldReturnGrowthStage_WhenExists()
        {
            var growthStageId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository response", () => {
                var expectedGrowthStage = GrowthStageMotherObject.CreateDefaultGrowthStage();
                _mockRepository.Setup(repo => repo.GetGrowthStageByIdAsync(growthStageId))
                              .ReturnsAsync(expectedGrowthStage);
            });

            var result = await AllureApi.Step($"Execute GetGrowthStageByIdAsync for ID: {growthStageId}", 
                async () => await _service.GetGrowthStageByIdAsync(growthStageId));

            await AllureApi.Step("Verify growth stage returned", () => {
                Assert.NotNull(result);
                Assert.Equal(growthStageId, result.Id);
                Assert.Equal("Germination", result.Name);
                Assert.Equal("Seed germination stage", result.Description);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetGrowthStageByIdAsync(growthStageId), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get growth stage by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetGrowthStageByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(repo => repo.GetGrowthStageByIdAsync(nonExistentId))
                              .ReturnsAsync((GrowthStage?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent growth stage with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.GetGrowthStageByIdAsync(nonExistentId));
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetGrowthStageByIdAsync(nonExistentId), Times.Once);
            });
        }
        #endregion

        #region GetAllGrowthStages Tests
        [Fact]
        [AllureName("Get all growth stages - should return all growth stages")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllGrowthStagesAsync_ShouldReturnAllGrowthStages()
        {
            await AllureApi.Step("Setup mock repository with multiple growth stages", () => {
                var growthStages = new List<GrowthStage>
                {
                    GrowthStageMotherObject.CreateGerminationStage(),
                    GrowthStageMotherObject.CreateVegetativeStage(),
                    GrowthStageMotherObject.CreateFloweringStage()
                };
                
                _mockRepository.Setup(repo => repo.GetAllGrowthStagesAsync())
                              .ReturnsAsync(growthStages);
            });

            var result = await AllureApi.Step("Execute GetAllGrowthStagesAsync", 
                async () => await _service.GetAllGrowthStagesAsync());

            await AllureApi.Step("Verify growth stages returned", () => {
                Assert.NotNull(result);
                Assert.Equal(3, result.Count());
                
                var germinationStage = result.First(gs => gs.Name == "Germination");
                var vegetativeStage = result.First(gs => gs.Name == "Vegetative");
                var floweringStage = result.First(gs => gs.Name == "Flowering");
                
                Assert.Equal("Seed germination stage", germinationStage.Description);
                Assert.Equal("Vegetative growth stage", vegetativeStage.Description);
                Assert.Equal("Flowering stage", floweringStage.Description);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetAllGrowthStagesAsync(), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get all growth stages - should return empty list when no growth stages")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllGrowthStagesAsync_ShouldReturnEmptyList_WhenNoGrowthStages()
        {
            await AllureApi.Step("Setup mock repository with empty list", () => {
                _mockRepository.Setup(repo => repo.GetAllGrowthStagesAsync())
                              .ReturnsAsync(new List<GrowthStage>());
            });

            var result = await AllureApi.Step("Execute GetAllGrowthStagesAsync", 
                async () => await _service.GetAllGrowthStagesAsync());

            await AllureApi.Step("Verify empty list returned", () => {
                Assert.NotNull(result);
                Assert.Empty(result);
            });
        }
        #endregion

        #region UpdateGrowthStage Tests
        [Fact]
        [AllureName("Update growth stage - should update growth stage when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdateGrowthStageAsync_ShouldUpdateGrowthStage_WhenValidData()
        {
            var growthStageId = Guid.NewGuid();
            
            await AllureApi.Step("Setup updated growth stage data", () => {
                var updatedGrowthStage = new GrowthStageBuilder()
                    .WithId(growthStageId)
                    .WithName("Updated Germination")
                    .WithDescription("Updated germination description")
                    .Build();
            });

            await AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.UpdateGrowthStageAsync(updatedGrowthStage))
                              .Returns(Task.CompletedTask);
            });

            await AllureApi.Step($"Execute UpdateGrowthStageAsync for ID: {growthStageId}", 
                async () => await _service.UpdateGrowthStageAsync(updatedGrowthStage));

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.UpdateGrowthStageAsync(updatedGrowthStage), Times.Once);
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
        #endregion
    }
}