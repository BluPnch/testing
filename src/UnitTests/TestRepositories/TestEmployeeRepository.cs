using System.ComponentModel;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UnitTests.Builders;
using UnitTests.MotherObjects;

namespace UnitTests.TestRepositories
{
    [AllureFeature("Employee Management")]
    [AllureStory("Employee Repository Operations")]
    public class TestEmployeeRepository : IClassFixture<RepositoryTestFixture>, IDisposable
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly EmployeeRepository _repository;

        public TestEmployeeRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new EmployeeRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        private async Task ClearDatabaseAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

            _context.EmployeePlants.RemoveRange(_context.EmployeePlants);
            _context.Plants.RemoveRange(_context.Plants);
            _context.Employees.RemoveRange(_context.Employees);
            _context.Administrators.RemoveRange(_context.Administrators);
            _context.Clients.RemoveRange(_context.Clients);

            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }

        public void Dispose()
        {
        }
        
        #region CreateEmployee Tests
        [Fact]
        [DisplayName("Create employee - should add employee to database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateEmployeeAsync_ShouldAddEmployeeToDatabase()
        {
            AdministratorDb administrator = null!;
            EmployeeDb employee = null!;

            // Настройка администратора
            await AllureApi.Step("Setup administrator", async () => {
                administrator = new AdministratorDbBuilder()
                    .WithSurname("admin")
                    .WithName("admin")
                    .Build();
            
                await _context.Administrators.AddAsync(administrator);
                await _context.SaveChangesAsync();
            });

            // Настройка сотрудника с администратором
            await AllureApi.Step("Setup employee with administrator", async () => {
                employee = new EmployeeDbBuilder()
                    .WithSurname("BBB")
                    .WithName("AAA")
                    .WithPatronymic("CCC")
                    .WithTask("Gardener")
                    .WithPlantDomain("Rose")
                    .WithPhoneNumber("1234567890")
                    .WithAdministrator(administrator)
                    .Build();
            
                await _context.Employees.AddAsync(employee); 
                await _context.SaveChangesAsync();
            });

            // Проверка создания сотрудника с корректными данными
            await AllureApi.Step("Verify employee created with correct data", async () => {
                var dbEmployee = await _context.Employees
                    .Include(e => e.Administrator)
                    .FirstOrDefaultAsync(e => e.Id == employee.Id);
            
                Assert.NotNull(dbEmployee);
                Assert.Equal(employee.Id, dbEmployee.Id);
                Assert.Equal("BBB", dbEmployee.Surname);
                Assert.Equal("AAA", dbEmployee.Name);
                Assert.Equal("CCC", dbEmployee.Patronymic);
                Assert.Equal("Gardener", dbEmployee.Task);
                Assert.Equal("Rose", dbEmployee.PlantDomain);
                Assert.Equal("1234567890", dbEmployee.PhoneNumber);
            
                Assert.NotNull(dbEmployee.Administrator);
                Assert.Equal(administrator.Id, dbEmployee.AdministratorId);
                Assert.Equal("admin", dbEmployee.Administrator.Surname);
                Assert.Equal("admin", dbEmployee.Administrator.Name);
            });
        }
        
        [Fact]
        [DisplayName("Create employee - should throw exception when admin not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateEmployeeAsync_ShouldThrowException_WhenAdminNotExists()
        {
            EmployeeDb employee = null!;

            // Настройка сотрудника с несуществующим администратором
            AllureApi.Step("Setup employee with non-existent administrator", () => {
                employee = new EmployeeDbBuilder()
                    .WithAdministratorId(Guid.NewGuid())
                    .Build();
            });

            // Попытка создания сотрудника с невалидным администратором
            await AllureApi.Step("Attempt to create employee with invalid administrator", async () => {
                await Assert.ThrowsAsync<DbUpdateException>(async () => 
                {
                    await _context.Employees.AddAsync(employee);
                    await _context.SaveChangesAsync();
                });
            });
        }
        #endregion

        #region GetAllEmployees Tests
        [Fact]
        [DisplayName("Get all employees - should return all employees")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllEmployeesAsync_ShouldReturnAllEmployees()
        {
            await AllureApi.Step("Setup administrator and employees", async () => {
                var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
        
                await _context.Administrators.AddAsync(administrator);
                await _context.SaveChangesAsync();

                var employees = new List<EmployeeDb>
                {
                    new EmployeeDbBuilder()
                        .WithSurname("BBB1")
                        .WithName("AAA1")
                        .WithPatronymic("A1.")
                        .WithTask("Gardener")
                        .WithPlantDomain("Peonies")
                        .WithAdministrator(administrator)
                        .Build(),
                    new EmployeeDbBuilder()
                        .WithSurname("BBB2")
                        .WithName("AAA2")
                        .WithPatronymic("A2.")
                        .WithTask("Biologist")
                        .WithPlantDomain("Rose")
                        .WithAdministrator(administrator)
                        .Build()
                };
        
                await _context.Employees.AddRangeAsync(employees);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step("Execute GetAllEmployeesAsync", 
                async () => await _repository.GetAllEmployeesAsync());

            AllureApi.Step("Verify 2 employees returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion

        #region GetEmployeeById Tests
        [Fact]
        [DisplayName("Get employee by ID - should return employee when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenExists()
        {
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
    
            await AllureApi.Step("Setup administrator and employee", async () => {
                await _context.Administrators.AddAsync(administrator);
                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetEmployeeByIdAsync for ID: {employee.Id}", 
                async () => await _repository.GetEmployeeByIdAsync(employee.Id));

            AllureApi.Step("Verify employee returned", () => {
                Assert.Equal(employee.Id, result.Id);
            });
        }

        [Fact]
        [DisplayName("Get employee by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetEmployeeByIdAsync_ShouldThrowEmployeeNotFoundException_WhenNotExists()
        {
            var employeeId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to get non-existent employee with ID: {employeeId}", async () => {
                await Assert.ThrowsAsync<EmployeeNotFoundException>(
                    () => _repository.GetEmployeeByIdAsync(employeeId));
            });
        }
        #endregion
        

        #region GetEmployeesByTask Tests
        [Fact]
        [DisplayName("Get employees by task - should return employees")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetEmployeesByTaskAsync_ShouldReturnEmployees()
        {
            var task = "Gardener";
            
            await AllureApi.Step($"Setup administrator and employees with task: {task}", async () => {
                var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
        
                await _context.Administrators.AddAsync(administrator);
                await _context.SaveChangesAsync();
                
                var employees = new List<EmployeeDb>
                {
                    new EmployeeDbBuilder()
                        .WithSurname("BBB1")
                        .WithName("AAA1")
                        .WithPatronymic("A1.")
                        .WithTask(task)
                        .WithPlantDomain("Rose2")
                        .WithAdministrator(administrator)
                        .Build(),
                    new EmployeeDbBuilder()
                        .WithSurname("BBB2")
                        .WithName("AAA2")
                        .WithPatronymic("A2.")
                        .WithTask(task)
                        .WithPlantDomain("Rose")
                        .WithAdministrator(administrator)
                        .Build()
                };
                
                await _context.Employees.AddRangeAsync(employees);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetEmployeesByTaskAsync for task: {task}", 
                async () => await _repository.GetEmployeesByTaskAsync(task));

            AllureApi.Step("Verify 2 employees returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion
        

        #region GetEmployeesByPlantDomain Tests
        [Fact]
        [DisplayName("Get employees by plant domain - should return employees")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetEmployeesByPlantDomainAsync_ShouldReturnEmployees()
        {
            var plantDomain = "Flowers";
            
            await AllureApi.Step($"Setup administrator and employees with plant domain: {plantDomain}", async () => {
                var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
        
                await _context.Administrators.AddAsync(administrator);
                await _context.SaveChangesAsync();
                
                var employees = new List<EmployeeDb>
                {
                    new EmployeeDbBuilder()
                        .WithSurname("BBB1")
                        .WithName("AAA1")
                        .WithPatronymic("A1.")
                        .WithTask("Gardener")
                        .WithPlantDomain(plantDomain)
                        .WithAdministrator(administrator)
                        .Build(),
                    new EmployeeDbBuilder()
                        .WithSurname("BBB2")
                        .WithName("AAA2")
                        .WithPatronymic("A2.")
                        .WithTask("Biologist")
                        .WithPlantDomain(plantDomain)
                        .WithAdministrator(administrator)
                        .Build()
                };
                
                await _context.Employees.AddRangeAsync(employees);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetEmployeesByPlantDomainAsync for domain: {plantDomain}", 
                async () => await _repository.GetEmployeesByPlantDomainAsync(plantDomain));

            AllureApi.Step("Verify 2 employees returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion
        

        #region GetEmployeeByPhoneNumber Tests
        [Fact]
        [DisplayName("Get employee by phone number - should return employee when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetEmployeeByPhoneNumberAsync_ShouldReturnEmployee_WhenExists()
        {
            var phoneNumber = "1234567890";
            
            await AllureApi.Step($"Setup administrator and employee with phone: {phoneNumber}", async () => {
                var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
                
                var employee = new EmployeeDbBuilder()
                    .WithSurname("BBB")
                    .WithName("AAA")
                    .WithPatronymic("CCC")
                    .WithTask("Gardener")
                    .WithPlantDomain("Rose")
                    .WithPhoneNumber(phoneNumber)
                    .WithAdministrator(administrator)
                    .Build();

                await _context.Administrators.AddAsync(administrator);
                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetEmployeeByPhoneNumberAsync for phone: {phoneNumber}", 
                async () => await _repository.GetEmployeeByPhoneNumberAsync(phoneNumber));

            AllureApi.Step("Verify phone number matches", () => {
                Assert.Equal(phoneNumber, result.PhoneNumber);
            });
        }

        [Fact]
        [DisplayName("Get employee by phone number - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetEmployeeByPhoneNumberAsync_ShouldThrowEmployeeNotFoundException_WhenNotExists()
        {
            var phoneNumber = "0000000000";

            await AllureApi.Step($"Attempt to get employee with non-existent phone: {phoneNumber}", async () => {
                await Assert.ThrowsAsync<EmployeeNotFoundException>(
                    () => _repository.GetEmployeeByPhoneNumberAsync(phoneNumber));
            });
        }
        #endregion
        

        #region GetPlantsByEmployeeId Tests
        [Fact]
        [DisplayName("Get plants by employee ID - should return plants")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantsByEmployeeIdAsync_ShouldReturnPlants()
        {
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
            PlantDb plant = null!;
    
            await AllureApi.Step("Setup employee-plant assignment", async () => {
                var client = new ClientDbBuilder()
                    .WithCompanyName("Client")
                    .WithPhoneNumber("1234567890")
                    .Build();
        
                plant = new PlantDbBuilder()
                    .WithPlantSpecie("Rose")
                    .WithPlantFamily("Rosaceae")
                    .WithClientId(client.Id)
                    .Build();
    
                await _context.Administrators.AddAsync(administrator);
                await _context.Clients.AddAsync(client);
                await _context.Employees.AddAsync(employee);
                await _context.Plants.AddAsync(plant);
        
                await _context.EmployeePlants.AddAsync(new EmployeePlantDb 
                { 
                    EmployeeId = employee.Id, 
                    PlantId = plant.Id 
                });
        
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetPlantsByEmployeeIdAsync for employee ID: {employee.Id}", 
                async () => await _repository.GetPlantsByEmployeeIdAsync(employee.Id));

            AllureApi.Step("Verify plant returned", () => {
                Assert.Single(result);
                Assert.Equal(plant.Id, result.First().Id);
            });
        }
        #endregion
        

        #region AssignEmployeeToPlant Tests
        [Fact]
        [DisplayName("Assign employee to plant - should not create duplicate assignment")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldNotCreateDuplicateAssignment()
        {
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
            PlantDb plant = null!;
    
            // Настройка сотрудника и растения
            await AllureApi.Step("Setup employee and plant", async () => {
                var client = new ClientDbBuilder()
                    .WithCompanyName("Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
        
                plant = new PlantDbBuilder()
                    .WithPlantSpecie("Rose")
                    .WithPlantFamily("Rosaceae")
                    .WithClientId(client.Id)
                    .Build();

                await _context.Administrators.AddAsync(administrator);
                await _context.Clients.AddAsync(client);
                await _context.Employees.AddAsync(employee);
                await _context.Plants.AddAsync(plant);
                await _context.SaveChangesAsync();
            });

            // Двойное назначение сотрудника растению
            await AllureApi.Step("Assign employee to plant twice", async () => {
                await _repository.AssignEmployeeToPlantAsync(employee.Id, plant.Id);
                await _repository.AssignEmployeeToPlantAsync(employee.Id, plant.Id);
            });

            // Проверка что создана только одна связь
            await AllureApi.Step("Verify only one assignment created", async () => {
                var assignments = await _context.EmployeePlants
                    .Where(ep => ep.EmployeeId == employee.Id && ep.PlantId == plant.Id)
                    .CountAsync();
    
                Assert.Equal(1, assignments);
            });
        }

        [Fact]
        [DisplayName("Assign employee to plant - should throw exception when employee not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowEmployeeNotFoundException_WhenEmployeeNotExists()
        {
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to assign non-existent employee {employeeId} to plant {plantId}", async () => {
                await Assert.ThrowsAsync<EmployeeNotFoundException>(
                    () => _repository.AssignEmployeeToPlantAsync(employeeId, plantId));
            });
        }

        [Fact]
        [DisplayName("Assign employee to plant - should throw exception when plant not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowPlantNotFoundException_WhenPlantNotExists()
        {
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
            var plantId = Guid.NewGuid();

            await AllureApi.Step("Setup employee", async () => {
                await _context.Administrators.AddAsync(administrator);
                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();
            });

            await AllureApi.Step($"Attempt to assign employee {employee.Id} to non-existent plant {plantId}", async () => {
                await Assert.ThrowsAsync<PlantNotFoundException>(
                    () => _repository.AssignEmployeeToPlantAsync(employee.Id, plantId));
            });
        }
        #endregion
    }
}