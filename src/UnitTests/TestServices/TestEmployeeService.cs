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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UnitTests.Builders;
using UnitTests.MotherObjects;

namespace UnitTests.TestServices
{
    [AllureFeature("Employee Service")]
    [AllureStory("Employee Management Operations")]
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
        [DisplayName("Create employee - should throw argument null when employee is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldThrowArgumentNull_WhenEmployeeIsNull()
        {
            await AllureApi.Step("Attempt to create null employee", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _service.CreateEmployeeAsync(null!));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("employee", exception.ParamName);
                    Assert.Contains("employee", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockEmployeeRepository.Verify(repo => repo.CreateEmployeeAsync(It.IsAny<Employee>()), Times.Never);
            });
        }
        
        [Fact]
        [DisplayName("Create employee - should create employee when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateEmployeeAsync_ShouldCreateEmployee_WhenValidData()
        {
            Employee employee = null!;
            Employee expectedEmployee = null!;
            
            AllureApi.Step("Setup valid employee data", () => {
                employee = EmployeeMotherObject.CreateDefaultEmployee();
                expectedEmployee = employee;
            });

            AllureApi.Step("Setup mock repository response", () => {
                _mockEmployeeRepository.Setup(repo => repo.CreateEmployeeAsync(employee))
                    .ReturnsAsync(expectedEmployee);
            });

            var result = await AllureApi.Step("Execute CreateEmployeeAsync", 
                async () => await _service.CreateEmployeeAsync(employee));

            AllureApi.Step("Verify employee created successfully", () => {
                Assert.Equal(expectedEmployee.Id, result.Id);
                Assert.Equal(expectedEmployee.Surname, result.Surname);
                Assert.Equal(expectedEmployee.Name, result.Name);
                Assert.Equal(expectedEmployee.PhoneNumber, result.PhoneNumber);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockEmployeeRepository.Verify(repo => repo.CreateEmployeeAsync(employee), Times.Once);
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
        [DisplayName("Create employee - should validate name when name contains numbers")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldValidateName_WhenNameContainsNumbers()
        {
            Employee invalidEmployee = null!;
            
            AllureApi.Step("Setup employee with invalid name containing numbers", () => {
                invalidEmployee = new EmployeeBuilder()
                    .WithName("AAA123")
                    .Build();
            });
            
            await AllureApi.Step("Attempt to create employee with invalid name", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateEmployeeAsync(invalidEmployee));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Имя может содержать только буквы и дефис", exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Create employee - should validate name when name is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldValidateName_WhenNameIsEmpty()
        {
            Employee invalidEmployee = null!;
            
            AllureApi.Step("Setup employee with empty name", () => {
                invalidEmployee = new EmployeeBuilder()
                    .WithName("")
                    .Build();
            });
            
            await AllureApi.Step("Attempt to create employee with empty name", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateEmployeeAsync(invalidEmployee));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Имя не может быть пустым", exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Create employee - should validate name when name is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldValidateName_WhenNameIsNull()
        {
            Employee invalidEmployee = null!;
            
            AllureApi.Step("Setup employee with null name", () => {
                invalidEmployee = new EmployeeBuilder()
                    .WithName(null!)
                    .Build();
            });
            
            await AllureApi.Step("Attempt to create employee with null name", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateEmployeeAsync(invalidEmployee));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Имя не может быть пустым", exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Create employee - should validate phone when phone is invalid")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldValidatePhone_WhenPhoneIsInvalid()
        {
            Employee invalidEmployee = null!;
            
            AllureApi.Step("Setup employee with invalid phone number", () => {
                invalidEmployee = new EmployeeBuilder()
                    .WithPhoneNumber("123")
                    .Build();
            });
            
            await AllureApi.Step("Attempt to create employee with invalid phone", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateEmployeeAsync(invalidEmployee));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Номер телефона должен быть в международном или местном формате", exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Create employee - should validate phone when phone is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldValidatePhone_WhenPhoneIsEmpty()
        {
            Employee invalidEmployee = null!;
            
            AllureApi.Step("Setup employee with empty phone number", () => {
                invalidEmployee = new EmployeeBuilder()
                    .WithPhoneNumber("")
                    .Build();
            });
            
            await AllureApi.Step("Attempt to create employee with empty phone", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateEmployeeAsync(invalidEmployee));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Номер телефона не может быть пустой", exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Create employee - should validate task length when task is too short")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldValidateTaskLength_WhenTaskIsTooShort()
        {
            Employee invalidEmployee = null!;
            
            AllureApi.Step("Setup employee with too short task description", () => {
                invalidEmployee = new EmployeeBuilder()
                    .WithTask("test")
                    .Build();
            });
            
            await AllureApi.Step("Attempt to create employee with short task", async () => {
                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => _service.CreateEmployeeAsync(invalidEmployee));
                
                AllureApi.Step("Verify validation error message", () => {
                    Assert.Contains("Задача должна быть от 5 до 500 символов", exception.Message);
                });
            });
        }
        #endregion

        #region AssignEmployeeToPlantAsync Tests
        [Fact]
        [DisplayName("Assign employee to plant - should throw exception when employee ID is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowArgumentException_WhenEmployeeIdIsEmpty()
        {
            await AllureApi.Step("Attempt to assign with empty employee ID", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.AssignEmployeeToPlantAsync(Guid.Empty, Guid.NewGuid()));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("employeeId", exception.ParamName);
                    Assert.Contains("cannot be empty", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockEmployeeRepository.Verify(
                    repo => repo.AssignEmployeeToPlantAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), 
                    Times.Never);
            });
        }
        
        [Fact]
        [DisplayName("Assign employee to plant - should throw exception when plant ID is empty")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowArgumentException_WhenPlantIdIsEmpty()
        {
            await AllureApi.Step("Attempt to assign with empty plant ID", async () => {
                var exception = await Assert.ThrowsAsync<ArgumentException>(
                    () => _service.AssignEmployeeToPlantAsync(Guid.NewGuid(), Guid.Empty));
        
                AllureApi.Step("Verify exception details", () => {
                    Assert.Equal("plantId", exception.ParamName);
                    Assert.Contains("cannot be empty", exception.Message);
                });
            });

            AllureApi.Step("Verify repository not called", () => {
                _mockEmployeeRepository.Verify(
                    repo => repo.AssignEmployeeToPlantAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), 
                    Times.Never);
            });
        }
        
        [Fact]
        [DisplayName("Assign employee to plant - should throw exception when employee not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowKeyNotFound_WhenEmployeeNotFound()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();
    
            AllureApi.Step("Setup mock to return null employee", () => {
                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                     .ReturnsAsync((Employee?)null);
            });
            
            await AllureApi.Step($"Attempt to assign non-existent employee {employeeId} to plant {plantId}", async () => {
                var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => _service.AssignEmployeeToPlantAsync(employeeId, plantId));
                
                AllureApi.Step("Verify exception contains employee ID", () => {
                    Assert.Contains(employeeId.ToString(), exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Assign employee to plant - should throw exception when plant not found")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowKeyNotFound_WhenPlantNotFound()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();
            Employee employee = null!;
            
            AllureApi.Step("Setup mock responses", () => {
                employee = EmployeeMotherObject.CreateDefaultEmployee();
                
                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                     .ReturnsAsync(employee);
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                                  .ReturnsAsync((Plant?)null);
            });
            
            await AllureApi.Step($"Attempt to assign employee {employeeId} to non-existent plant {plantId}", async () => {
                var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => _service.AssignEmployeeToPlantAsync(employeeId, plantId));
                
                AllureApi.Step("Verify exception contains plant ID", () => {
                    Assert.Contains(plantId.ToString(), exception.Message);
                });
            });
        }

        [Fact]
        [DisplayName("Assign employee to plant - should assign employee when valid data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task AssignEmployeeToPlantAsync_ShouldAssignEmployee_WhenValidData()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();
            Employee employee = null!;
            Plant plant = null!;
            
            AllureApi.Step("Setup employee and plant data", () => {
                employee = new EmployeeBuilder()
                    .WithId(employeeId)
                    .Build();
                    
                plant = new PlantBuilder()
                    .WithId(plantId)
                    .WithSpecie("Rose1")
                    .WithFamily("Rosaceae")
                    .Build();
            });

            AllureApi.Step("Setup mock repository responses", () => {
                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                     .ReturnsAsync(employee);
                _mockPlantRepository.Setup(repo => repo.GetPlantByIdAsync(plantId))
                                  .ReturnsAsync(plant);
            });

            await AllureApi.Step($"Execute AssignEmployeeToPlantAsync for employee {employeeId} and plant {plantId}", 
                async () => await _service.AssignEmployeeToPlantAsync(employeeId, plantId));

            AllureApi.Step("Verify all repository methods called", () => {
                _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);
                _mockPlantRepository.Verify(repo => repo.GetPlantByIdAsync(plantId), Times.Once);
                _mockEmployeeRepository.Verify(repo => repo.AssignEmployeeToPlantAsync(employeeId, plantId), Times.Once);
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
        [DisplayName("Delete employee - should delete employee when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task DeleteEmployeeAsync_ShouldDeleteEmployee_WhenExists()
        {
            var employeeId = Guid.NewGuid();
            Employee employee = null!;
            
            AllureApi.Step("Setup employee data", () => {
                employee = new EmployeeBuilder()
                    .WithId(employeeId)
                    .Build();
            });

            AllureApi.Step("Setup mock repository response", () => {
                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                                     .ReturnsAsync(employee);
            });

            await AllureApi.Step($"Execute DeleteEmployeeAsync for ID: {employeeId}", 
                async () => await _service.DeleteEmployeeAsync(employeeId));

            AllureApi.Step("Verify repository methods called", () => {
                _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);
                _mockEmployeeRepository.Verify(repo => repo.DeleteEmployeeAsync(employeeId), Times.Once);
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
        #endregion

        #region GetEmployee Tests
        [Fact]
        [DisplayName("Get employee by ID - should return employee when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenExists()
        {
            Employee expectedEmployee = null!;
    
            AllureApi.Step("Setup mock repository response", () => {
                expectedEmployee = EmployeeMotherObject.CreateDefaultEmployee();
                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(expectedEmployee.Id))
                    .ReturnsAsync(expectedEmployee);
            });

            var result = await AllureApi.Step($"Execute GetEmployeeByIdAsync for ID: {expectedEmployee.Id}", 
                async () => await _service.GetEmployeeByIdAsync(expectedEmployee.Id));

            AllureApi.Step("Verify employee returned", () => {
                Assert.NotNull(result);
                Assert.Equal(expectedEmployee.Id, result.Id);
                Assert.Equal("BBB", result.Surname);
                Assert.Equal("AAA", result.Name);
            });

            AllureApi.Step("Verify repository method called", () => {
                _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(expectedEmployee.Id), Times.Once);
            });
        }
        #endregion
    }
}