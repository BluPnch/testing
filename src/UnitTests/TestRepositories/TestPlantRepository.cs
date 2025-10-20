using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using DefaultNamespace;
using Domain.Exceptions;
using Domain.Models;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using UnitTests.Builders;
using UnitTests.MotherObjects;
using Xunit;

namespace UnitTests.TestRepositories
{
    public class TestPlantRepository : IClassFixture<RepositoryTestFixture>
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly PlantRepository _repository;

        public TestPlantRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new PlantRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        private async Task ClearDatabaseAsync()
        {
            _context.Plants.RemoveRange(_context.Plants);
            await _context.SaveChangesAsync();
        }
        
        

        private async Task<ClientDb> CreateTestClientAsync(Guid clientId = default)
        {
            var client = new ClientDb(
                clientId == default ? Guid.NewGuid() : clientId,
                "Test Client", 
                "test@email.com"
            );
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            return client;
        }

        #region CreatePlantAsync Tests
        [Fact]
        public async Task CreatePlantAsync_ShouldAddPlant()
        {
            
            var client = await CreateTestClientAsync();
            var plant = new PlantBuilder()
                .WithClientId(client.Id)
                .Build();

            
            var result = await _repository.CreatePlantAsync(plant, client.Id);

            // Assert
            var dbPlant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == plant.Id);
            Assert.NotNull(dbPlant);
            Assert.Equal(plant.Specie, dbPlant.Specie);
            Assert.Equal(plant.Family, dbPlant.Family);
            Assert.Equal(client.Id, dbPlant.ClientId);
        }

        [Fact]
        public async Task CreatePlantAsync_ShouldThrowArgumentNullException()
        {
            
            Plant? plant = null;
            var client = await CreateTestClientAsync();

            
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.CreatePlantAsync(plant!, client.Id));
        }
        #endregion

        #region GetPlantByIdAsync Tests
        [Fact]
        public async Task GetPlantByIdAsync_ShouldReturnPlant_WhenExists()
        {
            
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            var plantDb = new PlantDbBuilder()
                .WithId(plantId)
                .WithClientId(client.Id)
                .Build();
            
            await _context.Plants.AddAsync(plantDb);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetPlantByIdAsync(plantId);

            // Assert
            Assert.Equal(plantId, result.Id);
            Assert.Equal(plantDb.Specie, result.Specie);
        }

        [Fact]
        public async Task GetPlantByIdAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<PlantNotFoundException>(
                () => _repository.GetPlantByIdAsync(nonExistentId));
        }
        #endregion

        #region GetAllPlantsAsync Tests
        [Fact]
        public async Task GetAllPlantsAsync_ShouldReturnAllPlants()
        {
            
            var client = await CreateTestClientAsync();
            var plants = new List<PlantDb>
            {
                new PlantDbBuilder().WithClientId(client.Id).Build(),
                new PlantDbBuilder().WithClientId(client.Id).Build()
            };
            
            await _context.Plants.AddRangeAsync(plants);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAllPlantsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region UpdatePlantAsync Tests
        [Fact]
        public async Task UpdatePlantAsync_ShouldUpdatePlant()
        {
            
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            
            var originalPlant = new PlantDbBuilder()
                .WithId(plantId)
                .WithClientId(client.Id)
                .WithPlantSpecie("OldRose")
                .WithPlantFamily("OldRosaceae")
                .Build();
    
            await _context.Plants.AddAsync(originalPlant);
            await _context.SaveChangesAsync();

            var updatedPlant = new PlantBuilder()
                .WithId(plantId)
                .WithClientId(client.Id)
                .WithSpecie("NewRose")
                .WithFamily("NewRosaceae")
                .WithFlower(EnumFlowers.Zygomorphic)
                .WithFruit(EnumFruit.Capsule)
                .WithReproduction(EnumReproduction.Cutting)
                .Build();

            
            await _repository.UpdatePlantAsync(updatedPlant);

            // Assert
            var dbPlant = await _context.Plants.FindAsync(plantId);
            Assert.NotNull(dbPlant);
            Assert.Equal("NewRose", dbPlant!.Specie);
            Assert.Equal("NewRosaceae", dbPlant.Family);
            Assert.Equal(client.Id, dbPlant.ClientId); 
        }

        [Fact]
        public async Task UpdatePlantAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            
            var nonExistentPlant = PlantMotherObject.CreateDefaultPlant();

            
            await Assert.ThrowsAsync<PlantNotFoundException>(
                () => _repository.UpdatePlantAsync(nonExistentPlant));
        }

        [Fact]
        public async Task UpdatePlantAsync_ShouldThrowArgumentNullException()
        {
            
            Plant? plant = null;

            
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _repository.UpdatePlantAsync(plant!));
        }
        #endregion

        #region DeletePlantAsync Tests
        [Fact]
        public async Task DeletePlantAsync_ShouldRemovePlant()
        {
            
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            var plant = new PlantDbBuilder()
                .WithId(plantId)
                .WithClientId(client.Id)
                .Build();
            
            await _context.Plants.AddAsync(plant);
            await _context.SaveChangesAsync();

            
            await _repository.DeletePlantAsync(plantId);

            // Assert
            var dbPlant = await _context.Plants.FindAsync(plantId);
            Assert.Null(dbPlant);
        }

        [Fact]
        public async Task DeletePlantAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            
            var nonExistentId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<PlantNotFoundException>(
                () => _repository.DeletePlantAsync(nonExistentId));
        }
        #endregion

        #region GetPlantsByFamilyAsync Tests
        [Fact]
        public async Task GetPlantsByFamilyAsync_ShouldReturnPlants()
        {
            
            var client = await CreateTestClientAsync();
            var family = "Rosaceae";
            var plants = new List<PlantDb>
            {
                new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .WithPlantFamily(family)
                    .Build(),
                new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .WithPlantFamily(family)
                    .Build()
            };
            
            await _context.Plants.AddRangeAsync(plants);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetPlantsByFamilyAsync(family);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(family, p.Family));
        }
        #endregion

        #region GetPlantsBySpeciesAsync Tests
        [Fact]
        public async Task GetPlantsBySpeciesAsync_ShouldReturnPlants()
        {
            
            var client = await CreateTestClientAsync();
            var species = "Rose";
            var plants = new List<PlantDb>
            {
                new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .WithPlantSpecie(species)
                    .Build(),
                new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .WithPlantSpecie(species)
                    .Build()
            };
            
            await _context.Plants.AddRangeAsync(plants);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetPlantsBySpeciesAsync(species);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(species, p.Specie));
        }
        #endregion

        #region GetJournalRecordsByPlantIdAsync Tests
        [Fact]
        public async Task GetJournalRecordsByPlantIdAsync_ShouldReturnRecords()
        {
            
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            
            var plant = new PlantDbBuilder()
                .WithId(plantId)
                .WithClientId(client.Id)
                .Build();
            await _context.Plants.AddAsync(plant);
            await _context.SaveChangesAsync();
            
            
            var administratorId = Guid.NewGuid();
            var administrator = new AdministratorDb(administratorId, "AAA", "BBB", "CCC", "0000000000", "userAdmin");
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();

            var employeeId = Guid.NewGuid();
            var employee = new EmployeeDb(
                id: employeeId,
                surname: "BBB", 
                name: "AAA", 
                patronymic: "CCC", 
                task: "task", 
                plantDomain: "domain", 
                phoneNumber: "0000000000")
            {
                AdministratorId = administratorId 
            };
            await _context.Employees.AddAsync(employee);
            
            var growthStageId = Guid.NewGuid();
            var growthStage = new GrowthStageDb(growthStageId, "Test Stage", "Description");
            await _context.GrowthStages.AddAsync(growthStage);
            
            await _context.SaveChangesAsync();

            var records = new List<JournalRecordDb>
            {
                new JournalRecordDb(
                    id: Guid.NewGuid(),
                    plantHeight: 10.5,
                    fruitCount: 5,
                    condition: EnumCondition.Healthy,
                    date: DateTimeOffset.Now,
                    plantId: plantId,
                    growthStageId: growthStageId,
                    employeeId: employeeId),
                new JournalRecordDb(
                    id: Guid.NewGuid(),
                    plantHeight: 12.0,
                    fruitCount: 7,
                    condition: EnumCondition.Healthy,
                    date: DateTimeOffset.Now.AddDays(-1),
                    plantId: plantId,
                    growthStageId: growthStageId,
                    employeeId: employeeId)
            };
            
            await _context.JournalRecords.AddRangeAsync(records);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetJournalRecordsByPlantIdAsync(plantId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(plantId, r.PlantId));
        }
        #endregion

        #region GetSeedsByPlantIdAsync Tests
        [Fact]
        public async Task GetSeedsByPlantIdAsync_ShouldReturnSeeds()
        {
            
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            
            
            var plant = new PlantDbBuilder()
                .WithId(plantId)
                .WithClientId(client.Id)
                .Build();
            await _context.Plants.AddAsync(plant);
            await _context.SaveChangesAsync();

            var seeds = new List<SeedDb>
            {
                new SeedDb(Guid.NewGuid(), plantId, "Mature", EnumViability.Damaged, 
                    EnumLight.Medium, "Normal", 25),
                new SeedDb(Guid.NewGuid(), plantId, "Immature", EnumViability.Damaged, 
                    EnumLight.Low, "High", 20)
            };
            
            await _context.Seeds.AddRangeAsync(seeds);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetSeedsByPlantIdAsync(plantId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.Equal(plantId, s.PlantId));
        }
        #endregion

        #region GetPlantsByClientIdAsync Tests
        [Fact]
        public async Task GetPlantsByClientIdAsync_ShouldReturnPlants()
        {
            
            var client = await CreateTestClientAsync();
            var plants = new List<PlantDb>
            {
                new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .Build(),
                new PlantDbBuilder()
                    .WithClientId(client.Id)
                    .Build()
            };
    
            await _context.Plants.AddRangeAsync(plants);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetPlantsByClientIdAsync(client.Id);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, p => Assert.Equal(client.Id, p.ClientId));
        }
        #endregion
    }
}