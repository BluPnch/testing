using Allure.Xunit.Attributes;
using Allure.Net.Commons;
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
    [AllureFeature("Administrator Management")]
    [AllureStory("Administrator Repository")]
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
            await AllureApi.Step("Clear database", async () => {
                _context.Administrators.RemoveRange(_context.Administrators);
                await _context.SaveChangesAsync();
            });
        }

        #region GetAllAdministratorsAsync Tests
        [Fact]
        [AllureName("Get all administrators - should return all")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task GetAllAdministratorsAsync_ShouldReturnAllAdministrators()
        {
            await AllureApi.Step("Setup test data", async () => {
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
            });

            var result = await AllureApi.Step("Execute GetAllAdministratorsAsync", 
                async () => await _repository.GetAllAdministratorsAsync());

            await AllureApi.Step("Verify result", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion

        #region GetAdministratorByIdAsync Tests
        [Fact]
        [AllureName("Get administrator by ID - should return administrator when exists")]
        [AllureOwner("Development Team")]
        public async Task GetAdministratorByIdAsync_ShouldReturnAdministrator_WhenExists()
        {
            var adminId = Guid.NewGuid();
            
            await AllureApi.Step("Setup test data", async () => {
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
            });

            var result = await AllureApi.Step($"Get administrator by ID: {adminId}", 
                async () => await _repository.GetAdministratorByIdAsync(adminId));

            await AllureApi.Step("Verify administrator data", () => {
                Assert.Equal(adminId, result.Id);
                Assert.Equal("BBB", result.Username);
            });
        }

        [Fact]
        [AllureName("Get administrator by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        public async Task GetAdministratorByIdAsync_ShouldThrowAdministratorNotFoundException_WhenNotExists()
        {
            var adminId = Guid.NewGuid();

            var ex = await AllureApi.Step($"Attempt to get non-existent administrator: {adminId}", 
                async () => await Assert.ThrowsAsync<AdministratorNotFoundException>(
                    () => _repository.GetAdministratorByIdAsync(adminId)));
            
            await AllureApi.Step("Verify exception message", () => {
                Assert.Equal($"Administrator not found with id = {adminId}", ex.Message);
            });
        }
        #endregion

        // Аналогично исправьте остальные тесты...

        #region CreateAdministratorAsync Tests
        [Fact]
        [AllureName("Create administrator - should add to database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task CreateAdministratorAsync_ShouldAddAdministratorToDatabase()
        {
            var adminId = Guid.NewGuid();
            var phoneNumber = "1234567890";
            var surname = "BBB";
            var name = "AAA";
            var patronymic = "CCC";
            var username = "aaa";

            var result = await AllureApi.Step("Create administrator", 
                async () => await _repository.CreateAdministratorAsync(adminId, phoneNumber, surname, name, patronymic, username));

            await AllureApi.Step("Verify administrator created in database", async () => {
                var dbAdmin = await _context.Administrators.FirstOrDefaultAsync(a => a.Id == adminId);
                Assert.NotNull(dbAdmin);
                Assert.Equal(adminId, dbAdmin.Id);
                Assert.Equal(phoneNumber, dbAdmin.PhoneNumber);
                Assert.Equal(surname, dbAdmin.Surname);
                Assert.Equal(name, dbAdmin.Name);
                Assert.Equal(patronymic, dbAdmin.Patronymic);
            });
        }
        #endregion
    }
}