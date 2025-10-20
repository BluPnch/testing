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
    public class TestEmployeeRepository : IClassFixture<RepositoryTestFixture>
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
        
        

        #region CreateEmployee Tests
        [Fact]
        public async Task CreateEmployeeAsync_ShouldAddEmployeeToDatabase()
        {
            
            var administrator = new AdministratorDbBuilder()
                .WithSurname("admin")
                .WithName("admin")
                .Build();
    
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();
    
            var employee = new EmployeeDbBuilder()
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

            var dbEmployee = await _context.Employees
                .Include(e => e.Administrator)
                .FirstOrDefaultAsync(e => e.Id == employee.Id);
    
            // Assert
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
        }
        
        [Fact]
        public async Task CreateEmployeeAsync_ShouldThrowException_WhenAdminNotExists()
        {
            
            var employee = new EmployeeDbBuilder()
                .WithAdministratorId(Guid.NewGuid())
                .Build();

            
            await Assert.ThrowsAsync<DbUpdateException>(async () => 
            {
                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();
            });
        }
        #endregion

        #region GetAllEmployees Tests
        [Fact]
        public async Task GetAllEmployeesAsync_ShouldReturnAllEmployees()
        {
            
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

            
            var result = await _repository.GetAllEmployeesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region GetEmployeeById Tests
        [Fact]
        public async Task GetEmployeeByIdAsync_ShouldReturnEmployee_WhenExists()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
    
            await _context.Administrators.AddAsync(administrator);
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetEmployeeByIdAsync(employee.Id);

            // Assert
            Assert.Equal(employee.Id, result.Id);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ShouldThrowEmployeeNotFoundException_WhenNotExists()
        {
            
            var employeeId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<EmployeeNotFoundException>(
                () => _repository.GetEmployeeByIdAsync(employeeId));
        }
        #endregion
        

        #region GetEmployeesByTask Tests
        [Fact]
        public async Task GetEmployeesByTaskAsync_ShouldReturnEmployees()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var task = "Gardener";
    
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

            
            var result = await _repository.GetEmployeesByTaskAsync(task);

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion
        

        #region GetEmployeesByPlantDomain Tests
        [Fact]
        public async Task GetEmployeesByPlantDomainAsync_ShouldReturnEmployees()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var plantDomain = "Flowers";
    
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

            
            var result = await _repository.GetEmployeesByPlantDomainAsync(plantDomain);

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion
        

        #region GetEmployeeByPhoneNumber Tests
        [Fact]
        public async Task GetEmployeeByPhoneNumberAsync_ShouldReturnEmployee_WhenExists()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var phoneNumber = "1234567890";
            
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

            
            var result = await _repository.GetEmployeeByPhoneNumberAsync(phoneNumber);

            // Assert
            Assert.Equal(phoneNumber, result.PhoneNumber);
        }

        [Fact]
        public async Task GetEmployeeByPhoneNumberAsync_ShouldThrowEmployeeNotFoundException_WhenNotExists()
        {
            
            var phoneNumber = "0000000000";

            
            await Assert.ThrowsAsync<EmployeeNotFoundException>(
                () => _repository.GetEmployeeByPhoneNumberAsync(phoneNumber));
        }
        #endregion
        

        #region GetPlantsByEmployeeId Tests
        [Fact]
        public async Task GetPlantsByEmployeeIdAsync_ShouldReturnPlants()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
            
            var client = new ClientDbBuilder()
                .WithCompanyName("Client")
                .WithPhoneNumber("1234567890")
                .Build();
            
            var plant = new PlantDbBuilder()
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

            
            var result = await _repository.GetPlantsByEmployeeIdAsync(employee.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(plant.Id, result.First().Id);
        }
        #endregion
        

        #region AssignEmployeeToPlant Tests
        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldNotCreateDuplicateAssignment()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
            
            var client = new ClientDbBuilder()
                .WithCompanyName("Company")
                .WithPhoneNumber("1234567890")
                .Build();
            
            var plant = new PlantDbBuilder()
                .WithPlantSpecie("Rose")
                .WithPlantFamily("Rosaceae")
                .WithClientId(client.Id)
                .Build();

            await _context.Administrators.AddAsync(administrator);
            await _context.Clients.AddAsync(client);
            await _context.Employees.AddAsync(employee);
            await _context.Plants.AddAsync(plant);
            await _context.SaveChangesAsync();

            
            await _repository.AssignEmployeeToPlantAsync(employee.Id, plant.Id);
            
            await _repository.AssignEmployeeToPlantAsync(employee.Id, plant.Id);

            // Assert
            var assignments = await _context.EmployeePlants
                .Where(ep => ep.EmployeeId == employee.Id && ep.PlantId == plant.Id)
                .CountAsync();
        
            Assert.Equal(1, assignments);
        }

        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowEmployeeNotFoundException_WhenEmployeeNotExists()
        {
            
            var employeeId = Guid.NewGuid();
            var plantId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<EmployeeNotFoundException>(
                () => _repository.AssignEmployeeToPlantAsync(employeeId, plantId));
        }

        [Fact]
        public async Task AssignEmployeeToPlantAsync_ShouldThrowPlantNotFoundException_WhenPlantNotExists()
        {
            
            var administrator = EmployeeMotherObject.CreateDefaultAdministratorDb();
            var employee = EmployeeMotherObject.CreateEmployeeDbWithAdministrator(administrator);
            
            var plantId = Guid.NewGuid();

            await _context.Administrators.AddAsync(administrator);
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();

            
            await Assert.ThrowsAsync<PlantNotFoundException>(
                () => _repository.AssignEmployeeToPlantAsync(employee.Id, plantId));
        }
        #endregion
    }
}