using System.ComponentModel;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using UnitTests.Builder;
using Xunit;
using UnitTests.Builders;
using UnitTests.MotherObjects;
using UnitTests.ObjectMother;

namespace UnitTests.TestRepositories
{
    [AllureFeature("Growth Stage Management")]
    [AllureStory("Growth Stage Repository Operations")]
    public class TestGrowthStageRepository : IClassFixture<RepositoryTestFixture>, IDisposable
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly GrowthStageRepository _repository;

        public TestGrowthStageRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new GrowthStageRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        private async Task ClearDatabaseAsync()
        {
            _context.GrowthStages.RemoveRange(_context.GrowthStages);
            await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
        }
        
        #region CreateGrowthStage Tests
        [Fact]
        [DisplayName("Create growth stage - should add growth stage")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateGrowthStageAsync_ShouldAddGrowthStage()
        {
            GrowthStage growthStage = null!;

            // Создание объекта стадии роста
            AllureApi.Step("Setup growth stage", () => {
                growthStage = new GrowthStageBuilder()
                    .WithName("Germination")
                    .WithDescription("Initial growth stage")
                    .Build();
            });

            // Выполнение метода CreateGrowthStageAsync
            await AllureApi.Step("Execute CreateGrowthStageAsync", async () => {
                var result = await _repository.CreateGrowthStageAsync(growthStage);
            });

            // Проверка создания стадии роста в базе данных
            await AllureApi.Step("Verify growth stage created in database", async () => {
                var dbStage = await _context.GrowthStages.FirstOrDefaultAsync(gs => gs.Id == growthStage.Id);
                Assert.NotNull(dbStage);
                Assert.Equal(growthStage.Name, dbStage.Name);
                Assert.Equal(growthStage.Description, dbStage.Description);
            });
        }

        [Fact]
        [DisplayName("Create growth stage - should throw exception for null input")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreateGrowthStageAsync_ShouldThrowArgumentNullException()
        {
            GrowthStage? growthStage = null;

            // Подготовка null объекта стадии роста
            AllureApi.Step("Setup null growth stage", () => {
                growthStage = null;
            });

            // Попытка создания с null стадией роста
            await AllureApi.Step("Attempt to create with null growth stage", async () => {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _repository.CreateGrowthStageAsync(growthStage!));
            });
        }
        #endregion
        
        #region GetAllGrowthStages Tests
        [Fact]
        [DisplayName("Get all growth stages - should return all stages")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllGrowthStagesAsync_ShouldReturnAllStages()
        {
            await AllureApi.Step("Setup multiple growth stages", async () => {
                var stages = new List<GrowthStageDb>
                {
                    GrowthStageMotherObject.CreateGerminationStage(),
                    GrowthStageMotherObject.CreateVegetativeStage()
                };
                
                await _context.GrowthStages.AddRangeAsync(stages);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step("Execute GetAllGrowthStagesAsync", 
                async () => await _repository.GetAllGrowthStagesAsync());

            AllureApi.Step("Verify 2 stages returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion
        
        #region GetGrowthStageById Tests
        [Fact]
        [DisplayName("Get growth stage by ID - should return stage when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetGrowthStageByIdAsync_ShouldReturnStage_WhenExists()
        {
            var stageId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup growth stage with ID: {stageId}", async () => {
                var stage = new GrowthStageDbBuilder()
                    .WithId(stageId)
                    .WithName("Flowering")
                    .WithDescription("Bloom stage")
                    .Build();
                
                await _context.GrowthStages.AddAsync(stage);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetGrowthStageByIdAsync for ID: {stageId}", 
                async () => await _repository.GetGrowthStageByIdAsync(stageId));

            AllureApi.Step("Verify stage data", () => {
                Assert.Equal(stageId, result.Id);
                Assert.Equal("Flowering", result.Name);
                Assert.Equal("Bloom stage", result.Description);
            });
        }

        [Fact]
        [DisplayName("Get growth stage by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetGrowthStageByIdAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to get non-existent growth stage with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<GrowthStageNotFoundException>(
                    () => _repository.GetGrowthStageByIdAsync(nonExistentId));
            });
        }
        #endregion
        
        #region UpdateGrowthStage Tests
        [Fact]
        [DisplayName("Update growth stage - should update stage data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdateGrowthStageAsync_ShouldUpdateStage()
        {
            var stageId = Guid.NewGuid();
            GrowthStage updatedStage = null!;
    
            // Создание исходной стадии роста в базе данных
            await AllureApi.Step($"Setup original growth stage with ID: {stageId}", async () => {
                var originalStage = new GrowthStageDbBuilder()
                    .WithId(stageId)
                    .WithName("OldAAA")
                    .WithDescription("OldDescription")
                    .Build();
        
                await _context.GrowthStages.AddAsync(originalStage);
                await _context.SaveChangesAsync();
            });

            AllureApi.Step("Create updated growth stage", () => {
                updatedStage = new GrowthStageBuilder()
                    .WithId(stageId)
                    .WithName("NewAAA")
                    .WithDescription("NewDescription")
                    .Build();
            });

            await AllureApi.Step($"Execute UpdateGrowthStageAsync for ID: {stageId}", 
                async () => await _repository.UpdateGrowthStageAsync(updatedStage));

            await AllureApi.Step("Verify growth stage updated", async () => {
                var dbStage = await _context.GrowthStages.FindAsync(stageId);
                Assert.NotNull(dbStage);
                Assert.Equal("NewAAA", dbStage!.Name);
                Assert.Equal("NewDescription", dbStage.Description);
            });
        }

        [Fact]
        [DisplayName("Update growth stage - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdateGrowthStageAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            GrowthStage nonExistentStage = null!;

            AllureApi.Step("Create non-existent growth stage", () => {
                nonExistentStage = new GrowthStageBuilder().Build();
            });

            await AllureApi.Step("Attempt to update non-existent growth stage", async () => {
                await Assert.ThrowsAsync<GrowthStageNotFoundException>(
                    () => _repository.UpdateGrowthStageAsync(nonExistentStage));
            });
        }
        #endregion
        
        #region DeleteGrowthStage Tests
        [Fact]
        [DisplayName("Delete growth stage - should remove stage from database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task DeleteGrowthStageAsync_ShouldRemoveStage()
        {
            var stageId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup growth stage to delete with ID: {stageId}", async () => {
                var stage = new GrowthStageDbBuilder()
                    .WithId(stageId)
                    .WithName("ToDelete")
                    .WithDescription("Description")
                    .Build();
                
                await _context.GrowthStages.AddAsync(stage);
                await _context.SaveChangesAsync();
            });

            await AllureApi.Step($"Execute DeleteGrowthStageAsync for ID: {stageId}", 
                async () => await _repository.DeleteGrowthStageAsync(stageId));

            await AllureApi.Step("Verify growth stage deleted", async () => {
                var dbStage = await _context.GrowthStages.FindAsync(stageId);
                Assert.Null(dbStage);
            });
        }

        [Fact]
        [DisplayName("Delete growth stage - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task DeleteGrowthStageAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to delete non-existent growth stage with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<GrowthStageNotFoundException>(
                    () => _repository.DeleteGrowthStageAsync(nonExistentId));
            });
        }
        #endregion
    }
}