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
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");

            _context.JournalRecords.RemoveRange(_context.JournalRecords);
            _context.GrowthStages.RemoveRange(_context.GrowthStages);
            _context.Employees.RemoveRange(_context.Employees);
            _context.Plants.RemoveRange(_context.Plants);
            _context.Clients.RemoveRange(_context.Clients);
            _context.Administrators.RemoveRange(_context.Administrators);

            await _context.SaveChangesAsync();
            await _context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }
        
        

        private async Task<(Guid plantId, Guid growthStageId, Guid employeeId)> SetupDependenciesAsync()
        {
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
        }

        #region AddJournalRecord Tests
        [Fact]
        public async Task AddJournalRecordAsync_ShouldAddRecordToDatabase()
        {
            
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
            var journalRecord = new JournalRecordBuilder()
                .WithPlantId(plantId)
                .WithGrowthStageId(growthStageId)
                .WithEmployeeId(employeeId)
                .WithPlantHeight(10.5)
                .WithFruitCount(5)
                .WithCondition(EnumCondition.Healthy)
                .Build();

            
            var result = await _repository.AddJournalRecordAsync(journalRecord);

            // Assert
            var dbRecord = await _context.JournalRecords.FirstOrDefaultAsync(j => j.Id == journalRecord.Id);
            Assert.NotNull(dbRecord);
            Assert.Equal(journalRecord.PlantId, dbRecord.PlantId);
            Assert.Equal(journalRecord.GrowthStageId, dbRecord.GrowthStageId);
            Assert.Equal(journalRecord.EmployeeId, dbRecord.EmployeeId);
            Assert.Equal(journalRecord.PlantHeight, dbRecord.PlantHeight);
            Assert.Equal(journalRecord.FruitCount, dbRecord.FruitCount);
            Assert.Equal(journalRecord.Condition, dbRecord.Condition);
        }

        [Fact]
        public async Task AddJournalRecordAsync_ShouldThrowArgumentNullException_WhenRecordIsNull()
        {
            
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.AddJournalRecordAsync(null!));
        }
        #endregion
        

        #region GetAllJournalRecords Tests
        [Fact]
        public async Task GetAllJournalRecordsAsync_ShouldReturnAllRecords()
        {
            
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
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

            
            var result = await _repository.GetAllJournalRecordsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion
        

        #region GetJournalRecordById Tests
        [Fact]
        public async Task GetJournalRecordByIdAsync_ShouldReturnRecord_WhenExists()
        {
            
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
            var recordId = Guid.NewGuid();
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

            
            var result = await _repository.GetJournalRecordByIdAsync(recordId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(recordId, result!.Id);
            Assert.Equal(10.5, result.PlantHeight);
        }

        [Fact]
        public async Task GetJournalRecordByIdAsync_ShouldThrowJournalRecordNotFoundException_WhenNotExists()
        {
            
            var recordId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<JournalRecordNotFoundException>(
                () => _repository.GetJournalRecordByIdAsync(recordId));
        }
        #endregion
        

        #region UpdateJournalRecord Tests
        [Fact]
        public async Task UpdateJournalRecordAsync_ShouldUpdateRecord()
        {
            
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
            var recordId = Guid.NewGuid();
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

            var updatedRecord = new JournalRecordBuilder()
                .WithId(recordId)
                .WithPlantId(plantId)
                .WithGrowthStageId(growthStageId)
                .WithEmployeeId(employeeId)
                .WithPlantHeight(15.0)
                .WithFruitCount(8)
                .WithCondition(EnumCondition.BacterialDisease)
                .Build();

            
            await _repository.UpdateJournalRecordAsync(updatedRecord);

            // Assert
            var dbRecord = await _context.JournalRecords.FindAsync(recordId);
            Assert.NotNull(dbRecord);
            Assert.Equal(15.0, dbRecord!.PlantHeight);
            Assert.Equal(8, dbRecord.FruitCount);
            Assert.Equal(EnumCondition.BacterialDisease, dbRecord.Condition);
        }

        [Fact]
        public async Task UpdateJournalRecordAsync_ShouldThrowJournalRecordNotFoundException_WhenNotExists()
        {
            
            var nonExistentRecord = new JournalRecordBuilder().Build();

            
            await Assert.ThrowsAsync<JournalRecordNotFoundException>(
                () => _repository.UpdateJournalRecordAsync(nonExistentRecord));
        }
        #endregion
        

        #region DeleteJournalRecord Tests
        [Fact]
        public async Task DeleteJournalRecordAsync_ShouldRemoveRecord()
        {
            
            var (plantId, growthStageId, employeeId) = await SetupDependenciesAsync();
            
            var recordId = Guid.NewGuid();
            var record = new JournalRecordDbBuilder()
                .WithId(recordId)
                .WithPlantId(plantId)
                .WithGrowthStageId(growthStageId)
                .WithEmployeeId(employeeId)
                .Build();
            
            await _context.JournalRecords.AddAsync(record);
            await _context.SaveChangesAsync();

            
            await _repository.DeleteJournalRecordAsync(recordId);

            // Assert
            var dbRecord = await _context.JournalRecords.FindAsync(recordId);
            Assert.Null(dbRecord);
        }

        [Fact]
        public async Task DeleteJournalRecordAsync_ShouldThrowJournalRecordNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<JournalRecordNotFoundException>(
                () => _repository.DeleteJournalRecordAsync(nonExistentId));
        }
        #endregion
    }
}