using System.ComponentModel;
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
using UnitTests.ObjectMother;

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
        [DisplayName("Create journal record - should throw exception when journal record is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowArgumentNullException_WhenJournalRecordIsNull()
        {
            await AllureApi.Step("Attempt to create null journal record", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateJournalRecordAsync(null!));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("record", exception.ParamName);
                    Assert.Contains("record", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });

            AllureApi.Step("Remove logging verification", () => {
            });
        }

        [Fact]
        [DisplayName("Create journal record - should throw exception when plant not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowArgumentException_WhenPlantNotFound()
        {
            JournalRecord journalRecord = null!;

            AllureApi.Step("Setup journal record with non-existent plant", () => {
                journalRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
            });

            AllureApi.Step("Setup mock repository to return null plant", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(journalRecord.PlantId))
                    .ReturnsAsync((Plant?)null);
            });

            await AllureApi.Step("Attempt to create journal record for non-existent plant", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.CreateJournalRecordAsync(journalRecord));
        
                AllureApi.Step("Verify exception message", () => {
                    Assert.Contains("Растение не найдено", exception.Message);
                    Assert.Contains("PlantId", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [DisplayName("Create journal record - should validate when plant height is negative")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenPlantHeightIsNegative()
        {
            JournalRecord invalidRecord = null!;

            AllureApi.Step("Setup journal record with negative plant height", () => {
                invalidRecord = new JournalRecordBuilder()
                    .WithPlantHeight(-5)
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });
            
            await AllureApi.Step("Attempt to create journal record with negative height", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Высота растения должна быть положительным числом", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [DisplayName("Create journal record - should validate when fruit count is negative")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenFruitCountIsNegative()
        {
            JournalRecord invalidRecord = null!;

            AllureApi.Step("Setup journal record with negative fruit count", () => {
                invalidRecord = new JournalRecordBuilder()
                    .WithFruitCount(-1)
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });
            
            await AllureApi.Step("Attempt to create journal record with negative fruit count", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Количество плодов не может быть отрицательным", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [DisplayName("Create journal record - should validate when date is in future")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenDateIsInFuture()
        {
            JournalRecord invalidRecord = null!;

            AllureApi.Step("Setup journal record with future date", () => {
                invalidRecord = new JournalRecordBuilder()
                    .WithDate(DateTimeOffset.Now.AddDays(1))
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });
            
            await AllureApi.Step("Attempt to create journal record with future date", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Дата записи не может быть в будущем", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }

        [Fact]
        [DisplayName("Create journal record - should create journal record when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateJournalRecordAsync_ShouldCreateJournalRecord_WhenValidData()
        {
            Plant plant = null!;
            JournalRecord journalRecord = null!;

            AllureApi.Step("Setup valid journal record and plant data", () => {
                plant = PlantMotherObject.CreateDefaultPlant();
                journalRecord = new JournalRecordBuilder()
                    .WithPlantId(plant.Id)
                    .WithPlantHeight(15.5)
                    .WithFruitCount(8)
                    .WithDate(DateTimeOffset.Now.AddDays(-1))
                    .Build();
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plant.Id))
                                  .ReturnsAsync(plant);
                _mockRepository.Setup(repo => repo.AddJournalRecordAsync(journalRecord))
                              .ReturnsAsync(journalRecord);
            });

            var result = await AllureApi.Step("Execute CreateJournalRecordAsync", 
                async () => await _service.CreateJournalRecordAsync(journalRecord));

            AllureApi.Step("Verify journal record created successfully", () => {
                Assert.Equal(journalRecord.Id, result.Id);
                Assert.Equal(journalRecord.PlantId, result.PlantId);
                Assert.Equal(journalRecord.PlantHeight, result.PlantHeight);
                Assert.Equal(journalRecord.FruitCount, result.FruitCount);
                Assert.Equal(15.5, result.PlantHeight);
                Assert.Equal(8, result.FruitCount);
            });

            AllureApi.Step("Verify repository methods called", () => {
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plant.Id), Times.Once);
                _mockRepository.Verify(repo => repo.AddJournalRecordAsync(journalRecord), Times.Once);
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
        [DisplayName("Create journal record - should validate when plant height is zero")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateJournalRecordAsync_ShouldThrowValidationException_WhenPlantHeightIsZero()
        {
            JournalRecord invalidRecord = null!;

            AllureApi.Step("Setup journal record with zero plant height", () => {
                invalidRecord = new JournalRecordBuilder()
                    .WithPlantHeight(0)
                    .Build();
            });

            AllureApi.Step("Setup mock plant repository", () => {
                var plant = PlantMotherObject.CreateDefaultPlant();
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(invalidRecord.PlantId))
                                  .ReturnsAsync(plant);
            });

            await AllureApi.Step("Attempt to create journal record with zero height", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateJournalRecordAsync(invalidRecord));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Высота растения должна быть положительным числом", exception.Message);
                });
            });
        }
        #endregion

        #region UpdateJournalRecordAsync Tests
        [Fact]
        [DisplayName("Update journal record - should throw exception when journal record is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateJournalRecordAsync_ShouldThrowArgumentNullException_WhenJournalRecordIsNull()
        {
            await AllureApi.Step("Attempt to update null journal record", async () => {
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _service.UpdateJournalRecordAsync(null!));
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });

            AllureApi.Step("Remove logging verification", () => {
            });
        }

        [Fact]
        [DisplayName("Update journal record - should update journal record when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdateJournalRecordAsync_ShouldUpdateJournalRecord_WhenValidData()
        {
            JournalRecord journalRecord = null!;

            AllureApi.Step("Setup valid journal record for update", () => {
                journalRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
            });

            AllureApi.Step("Setup mock repository response", () => {
                _mockRepository.Setup(repo => repo.UpdateJournalRecordAsync(journalRecord))
                              .Returns(Task.CompletedTask);
            });

            await AllureApi.Step("Execute UpdateJournalRecordAsync", 
                async () => await _service.UpdateJournalRecordAsync(journalRecord));

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(journalRecord), Times.Once);
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
        [DisplayName("Update journal record - should validate data during update")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateJournalRecordAsync_ShouldThrowValidationException_WhenInvalidData()
        {
            JournalRecord invalidRecord = null!;

            AllureApi.Step("Setup invalid journal record for update", () => {
                invalidRecord = new JournalRecordBuilder()
                    .WithPlantHeight(-10)
                    .WithFruitCount(-5)
                    .Build();
            });

            await AllureApi.Step("Attempt to update journal record with invalid data", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                    _service.UpdateJournalRecordAsync(invalidRecord));
                
                AllureApi.Step("Verify validation errors", () => {
                    Assert.Contains("Высота растения должна быть положительным числом", exception.Message);
                    Assert.Contains("Количество плодов не может быть отрицательным", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockRepository.Verify(repo => repo.UpdateJournalRecordAsync(It.IsAny<JournalRecord>()), Times.Never);
            });
        }
        #endregion

        #region GetJournalRecord Tests
        [Fact]
        [DisplayName("Get journal record by ID - should return journal record when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetJournalRecordByIdAsync_ShouldReturnJournalRecord_WhenExists()
        {
            JournalRecord expectedRecord = null!;
    
            AllureApi.Step("Setup mock repository response", () => {
                expectedRecord = JournalRecordMotherObject.CreateDefaultJournalRecord();
                _mockRepository.Setup(repo => repo.GetJournalRecordByIdAsync(expectedRecord.Id))
                    .ReturnsAsync(expectedRecord);
            });

            var result = await AllureApi.Step($"Execute GetJournalRecordByIdAsync for ID: {expectedRecord.Id}", 
                async () => await _service.GetJournalRecordByIdAsync(expectedRecord.Id));

            AllureApi.Step("Verify journal record returned", () => {
                Assert.NotNull(result);
                Assert.Equal(expectedRecord.Id, result.Id);
                Assert.Equal(10.5, result.PlantHeight);
                Assert.Equal(5, result.FruitCount);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(repo => repo.GetJournalRecordByIdAsync(expectedRecord.Id), Times.Once);
            });
        }
        #endregion
    }
}