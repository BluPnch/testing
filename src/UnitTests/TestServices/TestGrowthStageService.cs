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
        public async Task CreateGrowthStageAsync_ShouldCreateGrowthStage_WhenValidData()
        {
            
            var growthStage = GrowthStageMotherObject.CreateDefaultGrowthStage();

            _mockRepository.Setup(repo => repo.CreateGrowthStageAsync(growthStage))
                          .ReturnsAsync(growthStage);

            
            var result = await _service.CreateGrowthStageAsync(growthStage);

            // Assert
            Assert.Equal(growthStage.Id, result.Id);
            Assert.Equal(growthStage.Name, result.Name);
            Assert.Equal(growthStage.Description, result.Description);
            _mockRepository.Verify(repo => repo.CreateGrowthStageAsync(growthStage), Times.Once);
        }

        [Fact]
        public async Task CreateGrowthStageAsync_ShouldThrowArgumentNullException_WhenGrowthStageIsNull()
        {
            
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateGrowthStageAsync(null));

            _mockRepository.Verify(repo => repo.CreateGrowthStageAsync(It.IsAny<GrowthStage>()), Times.Never);
        }
        #endregion
    }
}