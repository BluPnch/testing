using Allure.Xunit.Attributes;
using Allure.Net.Commons;
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
    [AllureFeature("Journal Record Service")]
    [AllureStory("Journal Record Management Operations")]
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
        [AllureName("Create journal record - should throw exception when journal record is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowArgumentNullException_WhenJournalRecordIsNull()
        {
            await AllureApi.Step("Attempt to create null journal record", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateJournalRecordAsync(null!));
                
                await AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("record", exception.ParamName);
                    Assert.Contains("record", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
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
        [AllureName("Create journal record - should throw exception when plant not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowArgumentException_WhenPlantNotFound()
        {
            await AllureApi.Step("Setup journal record with non-existent plant", () => {
                var journalRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
            });

            await AllureApi.Step("Setup mock repository to return null plant", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(journalRecord.PlantId))
                                  .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step("Attempt to create journal record for non-existent plant", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.CreateJournalRecordAsync(journalRecord));
                
                await AllureApi.Step("Verify exception message", () => {
                    Assert.Contains("Растение не найдено", exception.Message);
                    Assert.Contains(journalRecord.PlantId.ToString(), exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Create journal record - should validate when plant height is negative")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenPlantHeightIsNegative()
        {
            await AllureApi.Step("Setup journal record with negative plant height", () => {
                var invalidRecord = new JournalRecordBuilder()
                    .WithPlantHeight(-5)
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });
            
            await AllureApi.Step("Attempt to create journal record with negative height", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                await AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Высота растения должна быть положительным числом", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Create journal record - should validate when fruit count is negative")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenFruitCountIsNegative()
        {
            await AllureApi.Step("Setup journal record with negative fruit count", () => {
                var invalidRecord = new JournalRecordBuilder()
                    .WithFruitCount(-1)
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });
            
            await AllureApi.Step("Attempt to create journal record with negative fruit count", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                await AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Количество плодов не может быть отрицательным", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Create journal record - should validate when date is in future")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenDateIsInFuture()
        {
            await AllureApi.Step("Setup journal record with future date", () => {
                var invalidRecord = new JournalRecordBuilder()
                    .WithDate(DateTimeOffset.Now.AddDays(1))
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });
            
            await AllureApi.Step("Attempt to create journal record with future date", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                await AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Дата записи не может быть в будущем", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [AllureName("Create journal record - should create journal record when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task CreateJournalRecordAsync_ShouldCreateJournalRecord_WhenValidData()
        {
            await AllureApi.Step("Setup valid journal record and plant data", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                var journalRecord = new JournalRecordBuilder()
                    .WithPlantId(plant.Id)
                    .WithPlantHeight(15.5)
                    .WithFruitCount(8)
                    .WithDate(DateTimeOffset.Now.AddDays(-1))
                    .Build();
            });

            await AllureApi.Step("Setup mock repository responses", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plant.Id))
                                  .ReturnsAsync(plant);
                _mockRepository.Setup(repo => repo.AddJournalRecordAsync(journalRecord))
                              .ReturnsAsync(journalRecord);
            });

            var result = await AllureApi.Step("Execute CreateJournalRecordAsync", 
                async () => await _service.CreateJournalRecordAsync(journalRecord));

            await AllureApi.Step("Verify journal record created successfully", () => {
                Assert.Equal(journalRecord.Id, result.Id);
                Assert.Equal(journalRecord.PlantId, result.PlantId);
                Assert.Equal(journalRecord.PlantHeight, result.PlantHeight);
                Assert.Equal(journalRecord.FruitCount, result.FruitCount);
                Assert.Equal(15.5, result.PlantHeight);
                Assert.Equal(8, result.FruitCount);
            });

            await AllureApi.Step("Verify repository methods called", () => {
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plant.Id), Times.Once);
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(journalRecord), Times.Once);
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
        [AllureName("Create journal record - should validate when plant height is zero")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenPlantHeightIsZero()
        {
            await AllureApi.Step("Setup journal record with zero plant height", () => {
                var invalidRecord = new JournalRecordBuilder()
                    .WithPlantHeight(0)
                    .Build();
            });

            await AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });

            await AllureApi.Step("Attempt to create journal record with zero height", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                await AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Высота растения должна быть положительным числом", exception.Message);
                });
            });
        }
        #endregion

        #region UpdateJournalRecordAsync Tests
        [Fact]
        [AllureName("Update journal record - should throw exception when journal record is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateJournalRecordAsync_ShouldThrowArgumentNullException_WhenJournalRecordIsNull()
        {
            await AllureApi.Step("Attempt to update null journal record", async () => {
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _service.UpdateJournalRecordAsync(null));
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
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
        [AllureName("Update journal record - should update journal record when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task UpdateJournalRecordAsync_ShouldUpdateJournalRecord_WhenValidData()
        {
            await AllureApi.Step("Setup valid journal record for update", () => {
                var journalRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
            });

            await AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.UpdateJournalRecordAsync(journalRecord))
                              .Returns(Task.CompletedTask);
            });

            await AllureApi.Step("Execute UpdateJournalRecordAsync", 
                async () => await _service.UpdateJournalRecordAsync(journalRecord));

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(journalRecord), Times.Once);
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
        [AllureName("Update journal record - should validate data during update")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateJournalRecordAsync_ShouldThrowValidationException_WhenInvalidData()
        {
            await AllureApi.Step("Setup invalid journal record for update", () => {
                var invalidRecord = new JournalRecordBuilder()
                    .WithPlantHeight(-10)
                    .WithFruitCount(-5)
                    .Build();
            });

            await AllureApi.Step("Attempt to update journal record with invalid data", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                    _service.UpdateJournalRecordAsync(invalidRecord));
                
                await AllureApi.Step("Verify validation errors", () => {
                    Assert.Contains("Высота растения должна быть положительным числом", exception.Message);
                    Assert.Contains("Количество плодов не может быть отрицательным", exception.Message);
                });
            });

            await AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }
        #endregion

        #region GetJournalRecord Tests
        [Fact]
        [AllureName("Get journal record by ID - should return journal record when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task GetJournalRecordByIdAsync_ShouldReturnJournalRecord_WhenExists()
        {
            var recordId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository response", () => {
                var expectedRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
                _mockRepository.Setup(repo => repo.GetJournalRecordByIdAsync(recordId))
                              .ReturnsAsync(expectedRecord);
            });

            var result = await AllureApi.Step($"Execute GetJournalRecordByIdAsync for ID: {recordId}", 
                async () => await _service.GetJournalRecordByIdAsync(recordId));

            await AllureApi.Step("Verify journal record returned", () => {
                Assert.NotNull(result);
                Assert.Equal(recordId, result.Id);
                Assert.Equal(10.5, result.PlantHeight);
                Assert.Equal(5, result.FruitCount);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetJournalRecordByIdAsync(recordId), Times.Once);
            });
        }
        #endregion
    }
}