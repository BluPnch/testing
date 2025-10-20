using Domain.Interfaces.Repositories;
using Domain.Models;
using Application.Services;
using Moq;
using Xunit;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitTests.Builders;
using UnitTests.MotherObjects;


namespace UnitTests.TestServices
{
    public class TestEmployeeService
    {
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
        private readonly Mock<IPlantRepository> _mockPlantRepository;
        private readonly EmployeeService _service;
        private readonly Mock<ILogger<EmployeeService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly EmployeeValidator _employeeValidator;

        public TestEmployeeService()
        {
            _mockEmployeeRepository = new Mock<IEmployeeRepository>();
            _mockPlantRepository = new Mock<IPlantRepository>();
            _employeeValidator = new EmployeeValidator();
            _mockLogger = new Mock<ILogger<EmployeeService>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _service = new EmployeeService(
                _mockEmployeeRepository.Object,
                _mockPlantRepository.Object,
                _employeeValidator,
                _mockLogger.Object,
                _mockConfiguration.Object);
        }

        #region CreateEmployeeAsync Tests
        [Fact]
        public async Task CreateEmployeeAsync_ShouldThrowArgumentNull_WhenEmployeeIsNull()
        {
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateEmployeeAsync(null!));
    
            Assert.Equal("employee", ex.ParamName);
            _mockEmployeeRepository.Verify(repo => repo.CreateEmployeeAsync(It.IsAny<Employee>()), Times.Never);
        }
        
        [Fact]
        public async Task CreateEmployeeAsync_ShouldCreateEmployee_WhenValidData()
        {
            var employee = EmployeeMotherObject.CreateDefaultEmployee();
            var expectedEmployee = employee;

            _mockEmployeeRepository.Setup(repo => repo.CreateEmployeeAsync(employee))
                .ReturnsAsync(expectedEmployee);

            var result = await _service.CreateEmployeeAsync(employee);

            Assert.Equal(expectedEmployee.Id, result.Id);
            Assert.Equal(expectedEmployee.Surname, result.Surname);
            Assert.Equal(expectedEmployee.Name, result.Name);
            Assert.Equal(expectedEmployee.PhoneNumber, result.PhoneNumber);
            _mockEmployeeRepository.Verify(repo => repo.CreateEmployeeAsync(employee), Times.Once);
        }
        
        [Fact]
        public async Task CreateEmployeeAsync_ShouldValidateName_WhenNameContainsNumbers()
        {
            var invalidEmployee = new EmployeeBuilder()
                .WithName("AAA123")
                .Build();
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateEmployeeAsync(invalidEmployee));
            
            Assert.Contains("Имя может содержать только буквы и дефис", ex.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ShouldValidateName_WhenNameIsEmpty()
        {
            var invalidEmployee = new EmployeeBuilder()
                .WithName("")
                .Build();
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateEmployeeAsync(invalidEmployee));
            
            Assert.Contains("Имя не может быть пустым", ex.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ShouldValidateName_WhenNameIsNull()
        {
            var invalidEmployee = new EmployeeBuilder()
                .WithName(null!)
                .Build();
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateEmployeeAsync(invalidEmployee));
            
            Assert.Contains("Имя не может быть пустым", ex.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ShouldValidatePhone_WhenPhoneIsInvalid()
        {
            var invalidEmployee = new EmployeeBuilder()
                .WithPhoneNumber("123")
                .Build();
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateEmployeeAsync(invalidEmployee));
            
            Assert.Contains("Номер телефона должен быть в международном или местном формате", ex.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ShouldValidatePhone_WhenPhoneIsEmpty()
        {
            var invalidEmployee = new EmployeeBuilder()
                .WithPhoneNumber("")
                .Build();
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateEmployeeAsync(invalidEmployee));
            
            Assert.Contains("Номер телефона не может быть пустой", ex.Message);
        }

        [Fact]
        public async Task CreateEmployeeAsync_ShouldValidateTaskLength_WhenTaskIsTooShort()
        {
            var invalidEmployee = new EmployeeBuilder()
                .WithTask("test")
                .Build();
            
            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => _service.CreateEmployeeAsync(invalidEmployee));
            
            Assert.Contains("Задача должна быть от 5 до 500 символов", ex.Message);
        }
        #endregion

        #region AssignEmployeeToPlantAsync Tests
        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowArgumentException_WhenEmployeeIdIsEmpty()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AssignEmployeeToPlantAsync(Guid.Empty, Guid.NewGuid()));
    
            Assert.Equal("employeeId", ex.ParamName);
            Assert.Contains("cannot be empty", ex.Message);
            _mockEmployeeRepository.Verify(
                repo => repo.AssignEmployeeToPlantAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowArgumentException_WhenPlantIdIsEmpty()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.AssignEmployeeToPlantAsync(Guid.NewGuid(), Guid.Empty));
    
            Assert.Equal("plantId", ex.ParamName);
            Assert.Contains("cannot be empty", ex.Message);
            _mockEmployeeRepository.Verify(
                repo => repo.AssignEmployeeToPlantAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowKeyNotFound_WhenEmployeeNotFound()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();
    
            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                 .ReturnsAsync((Employee?)null);
            
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AssignEmployeeToPlantAsync(employeeId, plantId));
            
            Assert.Contains(employeeId.ToString(), ex.Message);
        }

        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowKeyNotFound_WhenPlantNotFound()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();
            var employee = EmployeeMotherObject.CreateDefaultEmployee();
            
            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                 .ReturnsAsync(employee);
            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                              .ReturnsAsync((Plant?)null);
            
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.AssignEmployeeToPlantAsync(employeeId, plantId));
            
            Assert.Contains(plantId.ToString(), ex.Message);
        }

        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldAssignEmployee_WhenValidData()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();
            
            var employee = new EmployeeBuilder()
                .WithId(employeeId)
                .Build();
                
            var plant = new PlantBuilder()
                .WithId(plantId)
                .WithSpecie("Rose1")
                .WithFamily("Rosaceae")
                .Build();

            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                 .ReturnsAsync(employee);
            _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                              .ReturnsAsync(plant);

            await _service.AssignEmployeeToPlantAsync(employeeId, plantId);

            _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);
            _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plantId), Times.Once);
            _mockEmployeeRepository.Verify(repo => repo.AssignEmployeeToPlantAsync(employeeId, plantId), Times.Once);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ShouldDeleteEmployee_WhenExists()
        {
            var employeeId = Guid.NewGuid();
            var employee = new EmployeeBuilder()
                .WithId(employeeId)
                .Build();

            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                 .ReturnsAsync(employee);

            await _service.DeleteEmployeeAsync(employeeId);

            _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);
            _mockEmployeeRepository.Verify(repo => repo.DeleteEmployeeAsync(employeeId), Times.Once);
        }
        #endregion
    }
}