using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using UnitTests.Builders;
using UnitTests.MotherObjects;

namespace UnitTests.TestServices
{
    public class TestJournalRecordService
    {
        private readonly Mock<IJournalRecordRepository> _mockRepository;
        private readonly Mock<IPlantRepository> _mockPlantRepository;
        private readonly JournalRecordService _service;
        private readonly JournalRecordValidator _journalRecordValidator;
        private readonly Mock<ILogger<JournalRecordService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public TestJournalRecordService()
        {
            _mockRepository = new Mock<IJournalRecordRepository>();
            _mockPlantRepository = new Mock<IPlantRepository>();
            _journalRecordValidator = new JournalRecordValidator();
            _mockLogger = new Mock<ILogger<JournalRecordService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new JournalRecordService(
                _mockRepository.Object,
                _mockPlantRepository.Object,
                _journalRecordValidator,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        #region CreateJournalRecordAsync Tests
        [Fact]
        public async Task CreateJournalRecordAsync_ShouldThrowArgumentNullException_WhenJournalRecordIsNull()
        {
            
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateJournalRecordAsync(null!));
            
            Assert.Equal("record", ex.ParamName);
            _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
        }

        [Fact]
        public async Task CreateJournalRecordAsync_ShouldThrowArgumentException_WhenPlantNotFound()
        {
            
            var journalRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
            
            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(journalRecord.PlantId))
                              .ReturnsAsync((Plant?)null);

            
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CreateJournalRecordAsync(journalRecord));
            
            Assert.Contains("Растение не найдено", ex.Message);
            _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
        }

        [Fact]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenPlantHeightIsNegative()
        {
            
            var invalidRecord = new JournalRecordBuilder()
                .WithPlantHeight(-5)
                .Build();
                
            var plant = PlantMotherObject.CreateDefaultPlant();

            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                              .ReturnsAsync(plant);
            
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateJournalRecordAsync(invalidRecord));
            
            Assert.Contains("Высота растения должна быть положительным числом", ex.Message);
        }

        [Fact]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenFruitCountIsNegative()
        {
            
            var invalidRecord = new JournalRecordBuilder()
                .WithFruitCount(-1)
                .Build();
                
            var plant = PlantMotherObject.CreateDefaultPlant();

            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                              .ReturnsAsync(plant);
            
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateJournalRecordAsync(invalidRecord));
            
            Assert.Contains("Количество плодов не может быть отрицательным", ex.Message);
        }

        [Fact]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenDateIsInFuture()
        {
            
            var invalidRecord = new JournalRecordBuilder()
                .WithDate(DateTimeOffset.Now.AddDays(1))
                .Build();
                
            var plant = PlantMotherObject.CreateDefaultPlant();

            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                              .ReturnsAsync(plant);
            
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateJournalRecordAsync(invalidRecord));
            
            Assert.Contains("Дата записи не может быть в будущем", ex.Message);
        }

        [Fact]
        public async Task CreateJournalRecordAsync_ShouldCreateJournalRecord_WhenValidData()
        {
            
            var plant = PlantMotherObject.CreateDefaultPlant();
            var journalRecord = new JournalRecordBuilder()
                .WithPlantId(plant.Id)
                .Build();

            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plant.Id))
                              .ReturnsAsync(plant);
            _mockRepository.Setup(repo => repo.AddJournalRecordAsync(journalRecord))
                          .ReturnsAsync(journalRecord);

            
            var result = await _service.CreateJournalRecordAsync(journalRecord);

            // Assert
            Assert.Equal(journalRecord.Id, result.Id);
            Assert.Equal(journalRecord.PlantId, result.PlantId);
            Assert.Equal(journalRecord.PlantHeight, result.PlantHeight);
            Assert.Equal(journalRecord.FruitCount, result.FruitCount);
            _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plant.Id), Times.Once);
            _mockRepository.Verify(repo => repo.AddJournalRecordAsync(journalRecord), Times.Once);
        }
        #endregion

        #region UpdateJournalRecordAsync Tests
        [Fact]
        public async Task UpdateJournalRecordAsync_ShouldThrowArgumentNullException_WhenJournalRecordIsNull()
        {
            
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateJournalRecordAsync(null));

            _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
        }

        [Fact]
        public async Task UpdateJournalRecordAsync_ShouldUpdateJournalRecord_WhenValidData()
        {
            
            var journalRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();

            _mockRepository.Setup(repo => repo.UpdateJournalRecordAsync(journalRecord))
                          .Returns(Task.CompletedTask);

            
            await _service.UpdateJournalRecordAsync(journalRecord);

            // Assert
            _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(journalRecord), Times.Once);
        }
        #endregion
    }
}