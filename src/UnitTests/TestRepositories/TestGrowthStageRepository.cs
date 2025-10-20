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

namespace UnitTests.TestRepositories
{
    public class TestGrowthStageRepository : IClassFixture<RepositoryTestFixture>
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
        
        

        #region CreateGrowthStage Tests
        [Fact]
        public async Task CreateGrowthStageAsync_ShouldAddGrowthStage()
        {
            
            var growthStage = new GrowthStageBuilder()
                .WithName("Germination")
                .WithDescription("Initial growth stage")
                .Build();

            
            var result = await _repository.CreateGrowthStageAsync(growthStage);

            // Assert
            var dbStage = await _context.GrowthStages.FirstOrDefaultAsync(gs => gs.Id == growthStage.Id);
            Assert.NotNull(dbStage);
            Assert.Equal(growthStage.Name, dbStage.Name);
            Assert.Equal(growthStage.Description, dbStage.Description);
        }

        [Fact]
        public async Task CreateGrowthStageAsync_ShouldThrowArgumentNullException()
        {
            
            GrowthStage? growthStage = null;

            
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.CreateGrowthStageAsync(growthStage!));
        }
        #endregion
        

        #region GetAllGrowthStages Tests
        [Fact]
        public async Task GetAllGrowthStagesAsync_ShouldReturnAllStages()
        {
            
            var stages = new List<GrowthStageDb>
            {
                GrowthStageMotherObject.CreateGerminationStage(),
                GrowthStageMotherObject.CreateVegetativeStage()
            };
            
            await _context.GrowthStages.AddRangeAsync(stages);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAllGrowthStagesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion

        
        #region GetGrowthStageById Tests
        [Fact]
        public async Task GetGrowthStageByIdAsync_ShouldReturnStage_WhenExists()
        {
            
            var stageId = Guid.NewGuid();
            var stage = new GrowthStageDbBuilder()
                .WithId(stageId)
                .WithName("Flowering")
                .WithDescription("Bloom stage")
                .Build();
            
            await _context.GrowthStages.AddAsync(stage);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetGrowthStageByIdAsync(stageId);

            // Assert
            Assert.Equal(stageId, result.Id);
            Assert.Equal("Flowering", result.Name);
        }

        [Fact]
        public async Task GetGrowthStageByIdAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<GrowthStageNotFoundException>(
                () => _repository.GetGrowthStageByIdAsync(nonExistentId));
        }
        #endregion

        
        #region UpdateGrowthStage Tests
        [Fact]
        public async Task UpdateGrowthStageAsync_ShouldUpdateStage()
        {
            
            var stageId = Guid.NewGuid();
            var originalStage = new GrowthStageDbBuilder()
                .WithId(stageId)
                .WithName("OldAAA")
                .WithDescription("OldDescription")
                .Build();
            
            await _context.GrowthStages.AddAsync(originalStage);
            await _context.SaveChangesAsync();

            var updatedStage = new GrowthStageBuilder()
                .WithId(stageId)
                .WithName("NewAAA")
                .WithDescription("NewDescription")
                .Build();

            
            await _repository.UpdateGrowthStageAsync(updatedStage);

            // Assert
            var dbStage = await _context.GrowthStages.FindAsync(stageId);
            Assert.NotNull(dbStage);
            Assert.Equal("NewAAA", dbStage!.Name);
            Assert.Equal("NewDescription", dbStage.Description);
        }

        [Fact]
        public async Task UpdateGrowthStageAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            
            var nonExistentStage = new GrowthStageBuilder().Build();

            
            await Assert.ThrowsAsync<GrowthStageNotFoundException>(
                () => _repository.UpdateGrowthStageAsync(nonExistentStage));
        }
        #endregion

        
        #region DeleteGrowthStage Tests
        [Fact]
        public async Task DeleteGrowthStageAsync_ShouldRemoveStage()
        {
            
            var stageId = Guid.NewGuid();
            var stage = new GrowthStageDbBuilder()
                .WithId(stageId)
                .WithName("ToDelete")
                .WithDescription("Description")
                .Build();
            
            await _context.GrowthStages.AddAsync(stage);
            await _context.SaveChangesAsync();

            
            await _repository.DeleteGrowthStageAsync(stageId);

            // Assert
            var dbStage = await _context.GrowthStages.FindAsync(stageId);
            Assert.Null(dbStage);
        }

        [Fact]
        public async Task DeleteGrowthStageAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<GrowthStageNotFoundException>(
                () => _repository.DeleteGrowthStageAsync(nonExistentId));
        }
        #endregion
    }
}