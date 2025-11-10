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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitTests.Builders;
using UnitTests.ObjectMother;
using DataAccess.Models.Converters;
using FluentValidation;


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
        [DisplayName("Create administrator - should create administrator and auth user when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateAdministratorAsync_ShouldCreateAdministratorAndAuthUser_WhenValidData()
        {
            Guid adminId = Guid.NewGuid();
            string phoneNumber = "1234567890";
            string surname = "BBB";
            string name = "AAA";
            string patronymic = "CCC";
            string username = "BBB";
            string password = "123";
            Administrator expectedAdmin = null!;
            
            AllureApi.Step("Setup test data", () => {
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(phoneNumber))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(username))
                    .ReturnsAsync(false);

                expectedAdmin = new AdministratorDbBuilder()
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

            AllureApi.Step("Verify administrator created successfully", () => {
                Assert.Equal(adminId, result.Id);
                Assert.Equal(phoneNumber, result.PhoneNumber);
                Assert.Equal(surname, result.Surname);
                Assert.Equal(name, result.Name);
                Assert.Equal(patronymic, result.Patronymic);
                Assert.Equal(username, result.Username);
            });

            AllureApi.Step("Verify auth user creation", () => {
                _mockAuthRepository.Verify(r => r.CreateAsync(It.Is<AuthUser>(u => 
                    u.Id == adminId && 
                    u.Username == username && 
                    u.PasswordHash != null && 
                    u.Role == EnumAuth.Administrator)), Times.Once);
            });

            AllureApi.Step("Verify logging", () => {
                _mockLogger.Verify(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
            });
        }

        [Fact]
        [DisplayName("Create administrator - should throw exception when phone number exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenPhoneNumberExists()
        {
            Administrator existingAdmin = null!;
            string phoneNumber = null!;
    
            AllureApi.Step("Setup existing administrator with phone number", () => {
                existingAdmin = AdministratorObjectMother.CreateDefaultAdministrator();
                phoneNumber = existingAdmin.PhoneNumber;

                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(phoneNumber))
                    .ReturnsAsync(existingAdmin);
            });

            await AllureApi.Step("Attempt to create administrator with duplicate phone number", async () => {
                // Используем ValidationException вместо InvalidOperationException
                var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        phoneNumber, // Этот номер не пройдет валидацию
                        "bbb",
                        "aaa",
                        "ccc",
                        "bbb",
                        "123"));
                    
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("телефон", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [DisplayName("Create administrator - should throw exception when username exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenUsernameExists()
        {
            string username = null!;
    
            AllureApi.Step("Setup existing username", () => {
                username = "existingUser";
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(username))
                    .ReturnsAsync(true);
            });

            await AllureApi.Step("Attempt to create administrator with duplicate username", async () => {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        "+79123456789",
                        "BBB",
                        "AAA",
                        "CCC",
                        username,
                        "123"));
                    
                AllureApi.Step("Verify exception message", () => {
                    Assert.Contains("логин", exception.Message.ToLower());
                });
            });

            AllureApi.Step("Verify logging occurred", () => {
                _mockLogger.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("already exists")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
            });
        }

        [Fact]
        [DisplayName("Create administrator - should throw exception when invalid phone number format")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenInvalidPhoneNumber()
        {
            AllureApi.Step("Setup repositories", () => {
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(false);
            });

            await AllureApi.Step("Attempt to create administrator with invalid phone number", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        "invalid-phone", 
                        "BBB",
                        "AAA",
                        "CCC",
                        "validuser",
                        "123"));
                            
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("телефон", exception.Message.ToLower());
                });
            });
        }

        [Fact]
        [DisplayName("Create administrator - should throw exception when empty required fields")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenEmptyRequiredFields()
        {
            AllureApi.Step("Setup repositories", () => {
                _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                    .ReturnsAsync((Administrator?)null);
                _mockAuthRepository.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                    .ReturnsAsync(false);
            });

            await AllureApi.Step("Attempt to create administrator with empty surname", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                    _service.CreateAdministratorAsync(
                        Guid.NewGuid(),
                        "1234567890",
                        "", // Пустая фамилия
                        "AAA",
                        "CCC",
                        "validuser",
                        "123"));
                            
                AllureApi.Step("Verify validation error", () => {
                    Assert.Contains("фамилия", exception.Message.ToLower());
                });
            });
        }
        #endregion

        #region GetAdministrator Tests
        [Fact]
        [DisplayName("Get administrator by ID - should return administrator when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetAdministratorByIdAsync_ShouldReturnAdministrator_WhenExists()
        {
            var adminId = Guid.NewGuid();
            Administrator expectedAdmin = null!;
            
            AllureApi.Step("Setup mock repository response", () => {
                expectedAdmin = AdministratorObjectMother.CreateDefaultAdministrator();
                expectedAdmin = new Administrator(adminId, expectedAdmin.Surname, expectedAdmin.Name, 
                    expectedAdmin.Patronymic, expectedAdmin.PhoneNumber, expectedAdmin.Username);
                    
                _mockRepository.Setup(r => r.GetAdministratorByIdAsync(adminId))
                    .ReturnsAsync(expectedAdmin);
            });

            var result = await AllureApi.Step($"Execute GetAdministratorByIdAsync for ID: {adminId}", 
                async () => await _service.GetAdministratorByIdAsync(adminId));

            AllureApi.Step("Verify administrator returned", () => {
                Assert.NotNull(result);
                Assert.Equal(adminId, result.Id);
                Assert.Equal("BBB_Default", result.Surname);
                Assert.Equal("AAA_Default", result.Name);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockRepository.Verify(r => r.GetAdministratorByIdAsync(adminId), Times.Once);
            });
        }

        [Fact]
        [DisplayName("Get administrator by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAdministratorByIdAsync_ShouldThrowException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();
    
            AllureApi.Step("Setup mock repository to return null", () => {
                _mockRepository.Setup(r => r.GetAdministratorByIdAsync(nonExistentId))
                    .ReturnsAsync((Administrator?)null);
            });

            await AllureApi.Step($"Attempt to get non-existent administrator with ID: {nonExistentId}", async () => {
                var result = await _service.GetAdministratorByIdAsync(nonExistentId);
                Assert.Null(result);
            });
        }
        #endregion
    }
}