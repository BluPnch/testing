using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using Domain.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitTests.Builders;
using UnitTests.ObjectMother;
using DataAccess.Models.Converters;

namespace UnitTests.TestServices
{
    [AllureFeature("Administrator Service")]
    [AllureStory("Administrator Management Operations")]
    public class TestAdministratorService
    {
        private readonly Mock<IAdministratorRepository> _mockRepository;
        private readonly Mock<IAuthUserRepository> _mockAuthRepository;
        private readonly Mock<ILogger<AdministratorService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AdministratorService _service;
        private readonly AdministratorValidator _administratorValidator;

        public TestAdministratorService()
        {
            _mockRepository = new Mock<IAdministratorRepository>();
            _mockAuthRepository = new Mock<IAuthUserRepository>();
            _mockLogger = new Mock<ILogger<AdministratorService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            _mockConfiguration.Setup(c => c["PasswordHash:SecretKey"]).Returns("testSecretKey12345");
            _mockConfiguration.Setup(c => c["PasswordHash:Iterations"]).Returns("10000");
            
            _administratorValidator = new AdministratorValidator();
            _service = new AdministratorService(
                _mockRepository.Object, 
                _administratorValidator, 
                _mockAuthRepository.Object,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        #region CreateAdministrator Tests
        [Fact]
        [AllureName("Create administrator - should create administrator and auth user when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task CreateAdministratorAsync_ShouldCreateAdministratorAndAuthUser_WhenValidData()
        {
            await AllureApi.Step("Setup test data", () => {
                var adminId = Guid.NewGuid();
                var phoneNumber = "1234567890";
                var surname = "BBB";
                var name = "AAA";
                var patronymic = "CCC";
                var username = "BBB";
                var password = "123";
            });

            await AllureApi.Step("Setup mock repository responses", () => {
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(phoneNumber))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(username))
                    .ReturnsAsync(false);

                var expectedAdmin = new AdministratorDbBuilder()
                    .WithId(adminId)
                    .WithSurname(surname)
                    .WithName(name)
                    .WithPatronymic(patronymic)
                    .WithPhoneNumber(phoneNumber)
                    .WithUsername(username)
                    .Build()
                    .ToDomain();

                _mockRepository.Setup(r => r.CreateAdministratorAsync(adminId, phoneNumber, surname, name, patronymic, username))
                    .ReturnsAsync(expectedAdmin);
            });

            var result = await AllureApi.Step("Execute CreateAdministratorAsync", async () => 
                await _service.CreateAdministratorAsync(adminId, phoneNumber, surname, name, patronymic, username, password));

            await AllureApi.Step("Verify administrator created successfully", () => {
                Assert.Equal(adminId, result.Id);
                Assert.Equal(phoneNumber, result.PhoneNumber);
                Assert.Equal(surname, result.Surname);
                Assert.Equal(name, result.Name);
                Assert.Equal(patronymic, result.Patronymic);
                Assert.Equal(username, result.Username);
            });

            await AllureApi.Step("Verify auth user creation", () => {
                _mockAuthRepository.Verify(r => r.CreateAsync(It.Is<AuthUser>(u => 
                    u.Id == adminId && 
                    u.Username == username && 
                    u.PasswordHash != null && 
                    u.Role == EnumAuth.Administrator)), Times.Once);
            });

            await AllureApi.Step("Verify logging", () => {
                _mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
            });
        }

        [Fact]
        [AllureName("Create administrator - should throw exception when phone number exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenPhoneNumberExists()
        {
            await AllureApi.Step("Setup existing administrator with phone number", () => {
                var existingAdmin = AdministratorObjectMother.CreateDefaultAdministrator();
                var phoneNumber = existingAdmin.PhoneNumber;

                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(phoneNumber))
                    .ReturnsAsync(existingAdmin);
            });

            await AllureApi.Step("Attempt to create administrator with duplicate phone number", async () => {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        phoneNumber,
                        "bbb",
                        "aaa",
                        "ccc",
                        "bbb",
                        "123"));
                        
                await AllureApi.Step("Verify exception message", () => {
                    Assert.Contains("phone number", exception.Message.ToLower());
                });
            });

            await AllureApi.Step("Verify logging occurred", () => {
                _mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
            });
        }

        [Fact]
        [AllureName("Create administrator - should throw exception when username exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenUsernameExists()
        {
            await AllureApi.Step("Setup existing username", () => {
                var username = "existingUser";
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(username))
                    .ReturnsAsync(true);
            });

            await AllureApi.Step("Attempt to create administrator with duplicate username", async () => {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        "1234567890",
                        "BBB",
                        "AAA",
                        "CCC",
                        username,
                        "123"));
                        
                await AllureApi.Step("Verify exception message", () => {
                    Assert.Contains("username", exception.Message.ToLower());
                });
            });

            await AllureApi.Step("Verify logging occurred", () => {
                _mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
            });
        }

        [Fact]
        [AllureName("Create administrator - should throw exception when invalid phone number format")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenInvalidPhoneNumber()
        {
            await AllureApi.Step("Setup invalid phone number", () => {
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(false);
            });

            await AllureApi.Step("Attempt to create administrator with invalid phone number", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        "invalid-phone", // Invalid phone number
                        "BBB",
                        "AAA",
                        "CCC",
                        "validuser",
                        "123"));
                        
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("phone", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [AllureName("Create administrator - should throw exception when empty required fields")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenEmptyRequiredFields()
        {
            await AllureApi.Step("Setup empty required fields", () => {
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(false);
            });

            await AllureApi.Step("Attempt to create administrator with empty surname", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        "1234567890",
                        "", // Empty surname
                        "AAA",
                        "CCC",
                        "validuser",
                        "123"));
                        
                await AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("surname", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region GetAdministrator Tests
        [Fact]
        [AllureName("Get administrator by ID - should return administrator when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task GetAdministratorByIdAsync_ShouldReturnAdministrator_WhenExists()
        {
            var adminId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository response", () => {
                var expectedAdmin = AdministratorObjectMother.CreateDefaultAdministrator();
                expectedAdmin = new Administrator(adminId, expectedAdmin.Surname, expectedAdmin.Name, 
                    expectedAdmin.Patronymic, expectedAdmin.PhoneNumber, expectedAdmin.Username);
                    
                _mockRepository.Setup(r => r.GetAdministratorByIdAsync(adminId))
                    .ReturnsAsync(expectedAdmin);
            });

            var result = await AllureApi.Step($"Execute GetAdministratorByIdAsync for ID: {adminId}", 
                async () => await _service.GetAdministratorByIdAsync(adminId));

            await AllureApi.Step("Verify administrator returned", () => {
                Assert.NotNull(result);
                Assert.Equal(adminId, result.Id);
                Assert.Equal("BBB", result.Surname);
                Assert.Equal("AAA", result.Name);
            });

            await AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(r => r.GetAdministratorByIdAsync(adminId), Times.Once);
            });
        }

        [Fact]
        [AllureName("Get administrator by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAdministratorByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
            
            await AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(r => r.GetAdministratorByIdAsync(nonExistentId))
                    .ReturnsAsync((Administrator?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent administrator with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<ArgumentException>(() =>
                    _service.GetAdministratorByIdAsync(nonExistentId));
            });
        }
        #endregion
    }
}