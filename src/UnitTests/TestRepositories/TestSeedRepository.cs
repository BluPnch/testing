using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Domain.Models.Enums;
using UnitTests.Builders;


namespace UnitTests.TestRepositories
{
    public class TestSeedRepository : IClassFixture<RepositoryTestFixture>
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly SeedRepository _repository;

        public TestSeedRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new SeedRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        private async Task ClearDatabaseAsync()
        {
            _context.Seeds.RemoveRange(_context.Seeds);
            await _context.SaveChangesAsync();
        }
        
        

        private async Task<Guid> SetupDependenciesAsync()
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
            
            await _context.SaveChangesAsync();
            
            return plant.Id;
        }

        [Fact]
        public async Task CreateSeedAsync_ShouldAddSeedToDatabase()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var seed = new SeedBuilder()
                .WithPlantId(plantId)
                .WithMaturity("Mature")
                .WithViability(EnumViability.Contaminated)
                .WithLightRequirements(EnumLight.Medium)
                .WithWaterRequirements("Normal")
                .WithTemperatureRequirements(25)
                .Build();

            
            var result = await _repository.CreateSeedAsync(seed);

            // Assert
            var dbSeed = await _context.Seeds.FirstOrDefaultAsync(s => s.Id == seed.Id);
            Assert.NotNull(dbSeed);
            Assert.Equal(seed.PlantId, dbSeed.PlantId);
            Assert.Equal(seed.Maturity, dbSeed.Maturity);
            Assert.Equal(seed.Viability, dbSeed.Viability);
            Assert.Equal(seed.LightRequirements, dbSeed.LightRequirements);
            Assert.Equal(seed.WaterRequirements, dbSeed.WaterRequirements);
            Assert.Equal(seed.TemperatureRequirements, dbSeed.TemperatureRequirements);
        }

        [Fact]
        public async Task CreateSeedAsync_ShouldThrowArgumentNullException_WhenSeedIsNull()
        {
            
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.CreateSeedAsync(null!));
        }

        [Fact]
        public async Task GetAllSeedsAsync_ShouldReturnAllSeeds()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var seeds = new List<SeedDb>
            {
                new SeedDbBuilder()
                    .WithPlantId(plantId)
                    .WithMaturity("Mature")
                    .WithViability(EnumViability.Contaminated)
                    .Build(),
                new SeedDbBuilder()
                    .WithPlantId(plantId)
                    .WithMaturity("Immature")
                    .WithViability(EnumViability.Damaged)
                    .Build()
            };
            
            await _context.Seeds.AddRangeAsync(seeds);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAllSeedsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSeedByIdAsync_ShouldReturnSeed_WhenExists()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var seedId = Guid.NewGuid();
            var seed = new SeedDbBuilder()
                .WithId(seedId)
                .WithPlantId(plantId)
                .WithMaturity("Mature")
                .WithViability(EnumViability.Contaminated)
                .Build();
            
            await _context.Seeds.AddAsync(seed);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetSeedByIdAsync(seedId);

            // Assert
            Assert.Equal(seedId, result.Id);
            Assert.Equal("Mature", result.Maturity);
        }

        [Fact]
        public async Task GetSeedByIdAsync_ShouldThrowSeedNotFoundException_WhenNotExists()
        {
            
            var seedId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<SeedNotFoundException>(
                () => _repository.GetSeedByIdAsync(seedId));
        }

        [Fact]
        public async Task UpdateSeedAsync_ShouldUpdateSeed()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var seedId = Guid.NewGuid();
            var originalSeed = new SeedDbBuilder()
                .WithId(seedId)
                .WithPlantId(plantId)
                .WithMaturity("OldMaturity")
                .WithViability(EnumViability.Contaminated)
                .WithLightRequirements(EnumLight.Medium)
                .WithWaterRequirements("OldWater")
                .WithTemperatureRequirements(25)
                .Build();
    
            await _context.Seeds.AddAsync(originalSeed);
            await _context.SaveChangesAsync();

            _context.Entry(originalSeed).State = EntityState.Detached;

            
            var updatedSeed = new SeedBuilder()
                .WithId(seedId)
                .WithPlantId(plantId)
                .WithMaturity("NewMaturity")
                .WithViability(EnumViability.Damaged)
                .WithLightRequirements(EnumLight.Low)
                .WithWaterRequirements("NewWater")
                .WithTemperatureRequirements(30)
                .Build();

            await _repository.UpdateSeedAsync(updatedSeed);

            // Assert
            var dbSeed = await _context.Seeds.FindAsync(seedId);
            Assert.NotNull(dbSeed);
            Assert.Equal("NewMaturity", dbSeed!.Maturity);
            Assert.Equal(EnumViability.Damaged, dbSeed.Viability);
            Assert.Equal(EnumLight.Low, dbSeed.LightRequirements);
            Assert.Equal("NewWater", dbSeed.WaterRequirements);
            Assert.Equal(30, dbSeed.TemperatureRequirements);
        }

        [Fact]
        public async Task DeleteSeedAsync_ShouldRemoveSeed()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var seedId = Guid.NewGuid();
            var seed = new SeedDbBuilder()
                .WithId(seedId)
                .WithPlantId(plantId)
                .WithMaturity("ToDelete")
                .WithViability(EnumViability.Contaminated)
                .Build();
            
            await _context.Seeds.AddAsync(seed);
            await _context.SaveChangesAsync();

            
            await _repository.DeleteSeedAsync(seedId);

            // Assert
            var dbSeed = await _context.Seeds.FindAsync(seedId);
            Assert.Null(dbSeed);
        }

        [Fact]
        public async Task DeleteSeedAsync_ShouldThrowSeedNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<SeedNotFoundException>(
                () => _repository.DeleteSeedAsync(nonExistentId));
        }

        [Fact]
        public async Task GetSeedsByMaturityAsync_ShouldReturnSeeds()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var maturity = "Mature";
            var seeds = new List<SeedDb>
            {
                new SeedDbBuilder()
                    .WithPlantId(plantId)
                    .WithMaturity(maturity)
                    .WithViability(EnumViability.Contaminated)
                    .Build(),
                new SeedDbBuilder()
                    .WithPlantId(plantId)
                    .WithMaturity(maturity)
                    .WithViability(EnumViability.Damaged)
                    .Build()
            };
            
            await _context.Seeds.AddRangeAsync(seeds);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetSeedsByMaturityAsync(maturity);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSeedsByMaturityAsync_ShouldReturnEmptyList_WhenNoMatches()
        {
            
            var maturity = "NonExistentMaturity";

            
            var result = await _repository.GetSeedsByMaturityAsync(maturity);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSeedsByViabilityAsync_ShouldReturnSeeds()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var viability = EnumViability.Contaminated;
            var seeds = new List<SeedDb>
            {
                new SeedDbBuilder()
                    .WithPlantId(plantId)
                    .WithMaturity("Mature1")
                    .WithViability(viability)
                    .Build(),
                new SeedDbBuilder()
                    .WithPlantId(plantId)
                    .WithMaturity("Mature2")
                    .WithViability(viability)
                    .Build()
            };
            
            await _context.Seeds.AddRangeAsync(seeds);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetSeedsByViabilityAsync(viability.ToString());

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetPlantBySeedIdAsync_ShouldReturnPlant()
        {
            
            var plantId = await SetupDependenciesAsync();
            
            var seedId = Guid.NewGuid();
            var seed = new SeedDbBuilder()
                .WithId(seedId)
                .WithPlantId(plantId)
                .WithMaturity("Mature")
                .WithViability(EnumViability.Contaminated)
                .Build();
            
            await _context.Seeds.AddAsync(seed);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetPlantBySeedIdAsync(seedId);

            // Assert
            Assert.Equal(plantId, result.Id);
            Assert.Equal("Rose", result.Specie);
        }

        [Fact]
        public async Task GetPlantBySeedIdAsync_ShouldThrowSeedNotFoundException_WhenSeedNotExists()
        {
            
            var seedId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<SeedNotFoundException>(
                () => _repository.GetPlantBySeedIdAsync(seedId));
        }
    }
}