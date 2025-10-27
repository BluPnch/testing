using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UnitTests.Builder;
using UnitTests.Builders;

namespace UnitTests.TestRepositories
{
    [AllureFeature("Journal Record Management")]
    [AllureStory("Journal Record Repository Operations")]
    public class TestJournalRecordRepository : IClassFixture<RepositoryTestFixture>
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly JournalRecordRepository _repository;

        public TestJournalRecordRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new JournalRecordRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        private async Task ClearDatabaseAsync()
        {
            await AllureApi.Step("Clear database with foreign key constraints", async () => {
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

                _context.JournalRecords.RemoveRange(_context.JournalRecords);
                _context.GrowthStages.RemoveRange(_context.GrowthStages);
                _context.Employees.RemoveRange(_context.Employees);
                _context.Plants.RemoveRange(_context.Plants);
                _context.Clients.RemoveRange(_context.Clients);
                _context.Administrators.RemoveRange(_context.Administrators);

                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
            });
        }
        
        private async Task<(Guid plantId, Guid growthStageId, Guid employeeId)> SetupDependenciesAsync()
        {
            return await AllureApi.Step("Setup test dependencies", async () => {
                var client = new ClientDbBuilder()
                    .WithCompanyName("Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
                await _context.Clients.AddAsync(client);
                
                var plant = new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .WithPlantSpecie("Rose")
                    .WithPlantFamily("Rosaceae")
                    .Build();
                await _context.Plants.AddAsync(plant);
                
                var admin = new AdministratorDbBuilder()
                    .WithSurname("admin")
                    .WithName("user")
                    .Build();
                await _context.Administrators.AddAsync(admin);
                
                var employee = new EmployeeDbBuilder()
                    .WithSurname("BBB")
                    .WithName("AAA")
                    .WithTask("Gardener")
                    .WithPlantDomain("Rose")
                    .WithAdministrator(admin)
                    .Build();
                await _context.Employees.AddAsync(employee);
                
                var growthStage = new GrowthStageDbBuilder()
                    .WithName("Flowering")
                    .WithDescription("Bloom stage")
                    .Build();
                await _context.GrowthStages.AddAsync(growthStage);
                
                await _context.SaveChangesAsync();
                
                return (plant.Id, growthStage.Id, employee.Id);
            });
        }

        #region AddJournalRecord Tests
        [Fact]
        [AllureName("Add journal record - should add record to database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task AddJournalRecordAsync_ShouldAddRecordToDatabase()
        {
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
            await AllureApi.Step("Create journal record with dependencies", () => {
                var journalRecord = new JournalRecordBuilder()
                    .WithPlantId(plantId)
                    .WithGrowthStageId(growthStageId)
                    .WithEmployeeId(employeeId)
                    .WithPlantHeight(10.5)
                    .WithFruitCount(5)
                    .WithCondition(EnumCondition.Healthy)
                    .Build();
            });

            await AllureApi.Step("Execute AddJournalRecordAsync", async () => {
                var result = await _repository.AddJournalRecordAsync(journalRecord);
            });

            await AllureApi.Step("Verify journal record created in database", async () => {
                var dbRecord = await _context.JournalRecords.FirstOrDefaultAsync(j => j.Id == journalRecord.Id);
                Assert.NotNull(dbRecord);
                Assert.Equal(journalRecord.PlantId, dbRecord.PlantId);
                Assert.Equal(journalRecord.GrowthStageId, dbRecord.GrowthStageId);
                Assert.Equal(journalRecord.EmployeeId, dbRecord.EmployeeId);
                Assert.Equal(journalRecord.PlantHeight, dbRecord.PlantHeight);
                Assert.Equal(journalRecord.FruitCount, dbRecord.FruitCount);
                Assert.Equal(journalRecord.Condition, dbRecord.Condition);
            });
        }

        [Fact]
        [AllureName("Add journal record - should throw exception when record is null")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task AddJournalRecordAsync_ShouldThrowArgumentNullException_WhenRecordIsNull()
        {
            await AllureApi.Step("Attempt to add null journal record", async () => {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _repository.AddJournalRecordAsync(null!));
            });
        }
        #endregion
        
        #region GetAllJournalRecords Tests
        [Fact]
        [AllureName("Get all journal records - should return all records")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllJournalRecordsAsync_ShouldReturnAllRecords()
        {
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
            await AllureApi.Step("Setup multiple journal records", async () => {
                var records = new List<JournalRecordDb>
                {
                    new JournalRecordDbBuilder()
                        .WithPlantId(plantId)
                        .WithGrowthStageId(growthStageId)
                        .WithEmployeeId(employeeId)
                        .WithPlantHeight(10.5)
                        .WithFruitCount(5)
                        .Build(),
                    new JournalRecordDbBuilder()
                        .WithPlantId(plantId)
                        .WithGrowthStageId(growthStageId)
                        .WithEmployeeId(employeeId)
                        .WithPlantHeight(12.0)
                        .WithFruitCount(7)
                        .Build()
                };
                
                await _context.JournalRecords.AddRangeAsync(records);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step("Execute GetAllJournalRecordsAsync", 
                async () => await _repository.GetAllJournalRecordsAsync());

            await AllureApi.Step("Verify 2 records returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion
        
        #region GetJournalRecordById Tests
        [Fact]
        [AllureName("Get journal record by ID - should return record when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task GetJournalRecordByIdAsync_ShouldReturnRecord_WhenExists()
        {
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            var recordId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup journal record with ID: {recordId}", async () => {
                var record = new JournalRecordDbBuilder()
                    .WithId(recordId)
                    .WithPlantId(plantId)
                    .WithGrowthStageId(growthStageId)
                    .WithEmployeeId(employeeId)
                    .WithPlantHeight(10.5)
                    .WithFruitCount(5)
                    .Build();
                
                await _context.JournalRecords.AddAsync(record);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetJournalRecordByIdAsync for ID: {recordId}", 
                async () => await _repository.GetJournalRecordByIdAsync(recordId));

            await AllureApi.Step("Verify record data", () => {
                Assert.NotNull(result);
                Assert.Equal(recordId, result!.Id);
                Assert.Equal(10.5, result.PlantHeight);
                Assert.Equal(5, result.FruitCount);
            });
        }

        [Fact]
        [AllureName("Get journal record by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetJournalRecordByIdAsync_ShouldThrowJournalRecordNotFoundException_WhenNotExists()
        {
            var recordId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to get non-existent journal record with ID: {recordId}", async () => {
                await Assert.ThrowsAsync<JournalRecordNotFoundException>(
                    () => _repository.GetJournalRecordByIdAsync(recordId));
            });
        }
        #endregion
        
        #region UpdateJournalRecord Tests
        [Fact]
        [AllureName("Update journal record - should update record data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task UpdateJournalRecordAsync_ShouldUpdateRecord()
        {
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            var recordId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup original journal record with ID: {recordId}", async () => {
                var originalRecord = new JournalRecordDbBuilder()
                    .WithId(recordId)
                    .WithPlantId(plantId)
                    .WithGrowthStageId(growthStageId)
                    .WithEmployeeId(employeeId)
                    .WithPlantHeight(10.5)
                    .WithFruitCount(5)
                    .Build();
                
                await _context.JournalRecords.AddAsync(originalRecord);
                await _context.SaveChangesAsync();
            });

            await AllureApi.Step("Create updated journal record", () => {
                var updatedRecord = new JournalRecordBuilder()
                    .WithId(recordId)
                    .WithPlantId(plantId)
                    .WithGrowthStageId(growthStageId)
                    .WithEmployeeId(employeeId)
                    .WithPlantHeight(15.0)
                    .WithFruitCount(8)
                    .WithCondition(EnumCondition.BacterialDisease)
                    .Build();
            });

            await AllureApi.Step($"Execute UpdateJournalRecordAsync for ID: {recordId}", 
                async () => await _repository.UpdateJournalRecordAsync(updatedRecord));

            await AllureApi.Step("Verify journal record updated", async () => {
                var dbRecord = await _context.JournalRecords.FindAsync(recordId);
                Assert.NotNull(dbRecord);
                Assert.Equal(15.0, dbRecord!.PlantHeight);
                Assert.Equal(8, dbRecord.FruitCount);
                Assert.Equal(EnumCondition.BacterialDisease, dbRecord.Condition);
            });
        }

        [Fact]
        [AllureName("Update journal record - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateJournalRecordAsync_ShouldThrowJournalRecordNotFoundException_WhenNotExists()
        {
            await AllureApi.Step("Create non-existent journal record", () => {
                var nonExistentRecord = new JournalRecordBuilder().Build();
            });

            await AllureApi.Step("Attempt to update non-existent journal record", async () => {
                await Assert.ThrowsAsync<JournalRecordNotFoundException>(
                    () => _repository.UpdateJournalRecordAsync(nonExistentRecord));
            });
        }
        #endregion
        
        #region DeleteJournalRecord Tests
        [Fact]
        [AllureName("Delete journal record - should remove record from database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.Critical)]
        public async Task DeleteJournalRecordAsync_ShouldRemoveRecord()
        {
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            var recordId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup journal record to delete with ID: {recordId}", async () => {
                var record = new JournalRecordDbBuilder()
                    .WithId(recordId)
                    .WithPlantId(plantId)
                    .WithGrowthStageId(growthStageId)
                    .WithEmployeeId(employeeId)
                    .Build();
                
                await _context.JournalRecords.AddAsync(record);
                await _context.SaveChangesAsync();
            });

            await AllureApi.Step($"Execute DeleteJournalRecordAsync for ID: {recordId}", 
                async () => await _repository.DeleteJournalRecordAsync(recordId));

            await AllureApi.Step("Verify journal record deleted", async () => {
                var dbRecord = await _context.JournalRecords.FindAsync(recordId);
                Assert.Null(dbRecord);
            });
        }

        [Fact]
        [AllureName("Delete journal record - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task DeleteJournalRecordAsync_ShouldThrowJournalRecordNotFoundException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to delete non-existent journal record with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<JournalRecordNotFoundException>(
                    () => _repository.DeleteJournalRecordAsync(nonExistentId));
            });
        }
        #endregion
    }
}