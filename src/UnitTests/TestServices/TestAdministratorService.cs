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
        public async Task CreateAdministratorAsync_ShouldCreateAdministratorAndAuthUser_WhenValidData()
        {
            
            var adminId = Guid.NewGuid();
            var phoneNumber = "1234567890";
            var surname = "BBB";
            var name = "AAA";
            var patronymic = "CCC";
            var username = "BBB";
            var password = "123";

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

            
            var result = await _service.CreateAdministratorAsync(adminId, phoneNumber, surname, name, patronymic, username, password);

            // Assert
            Assert.Equal(adminId, result.Id);
            Assert.Equal(phoneNumber, result.PhoneNumber);
            Assert.Equal(surname, result.Surname);
            Assert.Equal(name, result.Name);
            Assert.Equal(patronymic, result.Patronymic);
            Assert.Equal(username, result.Username);

            _mockAuthRepository.Verify(r => r.CreateAsync(It.Is<AuthUser>(u => 
                u.Id == adminId && 
                u.Username == username && 
                u.PasswordHash != null && 
                u.Role == EnumAuth.Administrator)), Times.Once);
                
            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenPhoneNumberExists()
        {
            
            var existingAdmin = AdministratorObjectMother.CreateDefaultAdministrator();
            var phoneNumber = existingAdmin.PhoneNumber;

            _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(phoneNumber))
                .ReturnsAsync(existingAdmin);

            
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateAdministratorAsync(
                    Guid.NewGuid(),
                    phoneNumber,
                    "bbb",
                    "aaa",
                    "ccc",
                    "bbb",
                    "123"));
                    
            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateAdministratorAsync_ShouldThrowException_WhenUsernameExists()
        {
            
            var username = "existingUser";
            _mockRepository.Setup(r => r.GetAdministratorByPhoneNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((Administrator?)null);
            _mockAuthRepository.Setup(r => r.UsernameExistsAsync(username))
                .ReturnsAsync(true);

            
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateAdministratorAsync(
                    Guid.NewGuid(),
                    "1234567890",
                    "BBB",
                    "AAA",
                    "CCC",
                    username,
                    "123"));
                    
            _mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.AtLeastOnce);
        }
        #endregion

    }
}