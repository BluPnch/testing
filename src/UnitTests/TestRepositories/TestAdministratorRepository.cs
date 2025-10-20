using Domain.Models;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using UnitTests.Builders;
using UnitTests.ObjectMother;
using Xunit;

namespace UnitTests.TestRepositories
{
    public class TestAdministratorRepository : IClassFixture<RepositoryTestFixture>
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly AdministratorRepository _repository;

        public TestAdministratorRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new AdministratorRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        private async Task ClearDatabaseAsync()
        {
            _context.Administrators.RemoveRange(_context.Administrators);
            await _context.SaveChangesAsync();
        }

        
        
        
        #region GetAllAdministratorsAsync Tests
        [Fact]
        public async Task GetAllAdministratorsAsync_ShouldReturnAllAdministrators()
        {
            
            var administrators = new List<AdministratorDb>
            {
                new AdministratorDbBuilder()
                    .WithSurname("BBB")
                    .WithName("AAA")
                    .WithPatronymic("CCC")
                    .WithPhoneNumber("1234567890")
                    .WithUsername("BBB")
                    .Build(),
                    
                new AdministratorDbBuilder()
                    .WithSurname("bbb")
                    .WithName("aaa")
                    .WithPatronymic(null)
                    .WithPhoneNumber("1111111111")
                    .WithUsername("bbb")
                    .Build()
            };
            
            await _context.Administrators.AddRangeAsync(administrators);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAllAdministratorsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region GetAdministratorByIdAsync Tests
        [Fact]
        public async Task GetAdministratorByIdAsync_ShouldReturnAdministrator_WhenExists()
        {
            
            var adminId = Guid.NewGuid();
            var administrator = new AdministratorDbBuilder()
                .WithId(adminId)
                .WithSurname("BBB")
                .WithName("AAA")
                .WithPatronymic("CCC")
                .WithPhoneNumber("1234567890")
                .WithUsername("BBB")
                .Build();
            
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAdministratorByIdAsync(adminId);

            // Assert
            Assert.Equal(adminId, result.Id);
            Assert.Equal("BBB", result.Username);
        }

        [Fact]
        public async Task GetAdministratorByIdAsync_ShouldThrowAdministratorNotFoundException_WhenNotExists()
        {
            
            var adminId = Guid.NewGuid();

            
            var ex = await Assert.ThrowsAsync<AdministratorNotFoundException>(
                () => _repository.GetAdministratorByIdAsync(adminId));
            
            Assert.Equal($"Administrator not found with id = {adminId}", ex.Message);
        }
        #endregion

        #region GetAdministratorByPhoneNumberAsync Tests
        [Fact]
        public async Task GetAdministratorByPhoneNumberAsync_ShouldReturnAdministrator_WhenExists()
        {
            
            var phoneNumber = "1234567890";
            var administrator = AdministratorObjectMother.CreateAdministratorDbWithPhone(phoneNumber);
            
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAdministratorByPhoneNumberAsync(phoneNumber);

            // Assert
            Assert.Equal(phoneNumber, result.PhoneNumber);
            Assert.Equal("user_WithPhone_Db", result.Username);
        }

        [Fact]
        public async Task GetAdministratorByPhoneNumberAsync_ShouldThrowArgumentException_WhenPhoneNumberIsEmpty()
        {
            
            await Assert.ThrowsAsync<ArgumentException>(
                () => _repository.GetAdministratorByPhoneNumberAsync(""));
        }

        [Fact]
        public async Task GetAdministratorByPhoneNumberAsync_ShouldThrowAdministratorNotFoundException_WhenNotExists()
        {
            
            await ClearDatabaseAsync();
            var phoneNumber = "1234567890";

            
            var ex = await Assert.ThrowsAsync<AdministratorNotFoundException>(
                () => _repository.GetAdministratorByPhoneNumberAsync(phoneNumber));
            
            Assert.Equal($"Administrator with phone number {phoneNumber} not found", ex.Message);
        }
        #endregion

        #region GetAdministratorByFullNameAsync Tests
        [Fact]
        public async Task GetAdministratorByFullNameAsync_ShouldReturnAdministrator_WhenExists()
        {
            
            var surname = "BBB";
            var name = "AAA";
            var patronymic = "CCC";
            
            var administrator = AdministratorObjectMother.CreateAdministratorDbWithFullName(surname, name, patronymic);
            
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAdministratorByFullNameAsync(surname, name, patronymic);

            // Assert
            Assert.Equal(surname, result.Surname);
            Assert.Equal(name, result.Name);
            Assert.Equal(patronymic, result.Patronymic);
            Assert.Equal("aaabbb", result.Username);
        }

        [Fact]
        public async Task GetAdministratorByFullNameAsync_ShouldThrowAdministratorNotFoundException_WhenNotExists()
        {
            
            var surname = "BBB";
            var name = "AAA";
            var patronymic = "CCC";

            
            var ex = await Assert.ThrowsAsync<AdministratorNotFoundException>(
                () => _repository.GetAdministratorByFullNameAsync(surname, name, patronymic));
    
            Assert.Equal($"Administrator with {surname} {name} {patronymic} not found", ex.Message);
        }
        #endregion

        #region CreateAdministratorAsync Tests
        [Fact]
        public async Task CreateAdministratorAsync_ShouldAddAdministratorToDatabase()
        {
            
            var adminId = Guid.NewGuid();
            var phoneNumber = "1234567890";
            var surname = "BBB";
            var name = "AAA";
            var patronymic = "CCC";
            var username = "aaa";
        
            
            var result = await _repository.CreateAdministratorAsync(adminId, phoneNumber, surname, name, patronymic, username);
        
            // Assert
            var dbAdmin = await _context.Administrators.FirstOrDefaultAsync(a => a.Id == adminId);
            Assert.NotNull(dbAdmin);
            Assert.Equal(adminId, dbAdmin.Id);
            Assert.Equal(phoneNumber, dbAdmin.PhoneNumber);
            Assert.Equal(surname, dbAdmin.Surname);
            Assert.Equal(name, dbAdmin.Name);
            Assert.Equal(patronymic, dbAdmin.Patronymic);
        }
        #endregion

        #region UpdateAdministratorAsync Tests
        [Fact]
        public async Task UpdateAdministratorAsync_ShouldUpdateAdministratorInDatabase()
        {
            
            var adminId = Guid.NewGuid();
            var existingAdmin = new AdministratorDbBuilder()
                .WithId(adminId)
                .WithSurname("OldBBB")
                .WithName("OldAAA")
                .WithPatronymic("OldPatronymic")
                .WithPhoneNumber("oldNumber")
                .WithUsername("oldUserAAA")
                .Build();
                
            await _context.Administrators.AddAsync(existingAdmin);
            await _context.SaveChangesAsync();

            var updatedAdmin = new Administrator(
                adminId, "NewBBB", "NewAAA", "NewCCC", "NewNumb", "newAAA");
                
            
            var result = await _repository.UpdateAdministratorAsync(updatedAdmin);

            // Assert
            _context.Entry(existingAdmin).State = EntityState.Detached; 
            var dbAdmin = await _context.Administrators.FindAsync(adminId);
            
            Assert.NotNull(dbAdmin);
            Assert.Equal("NewNumb", dbAdmin.PhoneNumber);
            Assert.Equal("NewBBB", dbAdmin.Surname);
            Assert.Equal("NewAAA", dbAdmin.Name);
            Assert.Equal("NewCCC", dbAdmin.Patronymic);
            Assert.Equal("newAAA", dbAdmin.Username);
        }

        [Fact]
        public async Task UpdateAdministratorAsync_ShouldThrowAdministratorNotFoundException_WhenNotExists()
        {
            
            var adminId = Guid.NewGuid();
            var updatedAdmin = AdministratorObjectMother.CreateDefaultAdministrator();
            updatedAdmin = new Administrator(adminId, updatedAdmin.Surname, updatedAdmin.Name, 
                updatedAdmin.Patronymic, updatedAdmin.PhoneNumber, updatedAdmin.Username);

            
            var ex = await Assert.ThrowsAsync<AdministratorNotFoundException>(
                () => _repository.UpdateAdministratorAsync(updatedAdmin));
            
            Assert.Equal($"Administrator with id {adminId} not found", ex.Message);
        }
        #endregion
    }
}