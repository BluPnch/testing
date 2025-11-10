using Allure.Xunit.Attributes;
using Allure.Net.Commons;
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
using System.ComponentModel;
using UnitTests.ObjectMother;

namespace UnitTests.TestRepositories
{
    [AllureFeature("Plant Management")]
    [AllureStory("Plant Repository Operations")]
    public class TestPlantRepository : IClassFixture<RepositoryTestFixture>, IDisposable
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

        public void Dispose()
        {
        }
        
        
        private async Task<ClientDb> CreateTestClientAsync(Guid clientId = default)
        {
            return await AllureApi.Step("Create test client", async () => {
                var client = new ClientDb(
                    clientId == default ? Guid.NewGuid() : clientId,
                    "Test Client", 
                    "test@email.com"
                );
                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();
                return client;
            });
        }

        #region CreatePlantAsync Tests
        [Fact]
        [DisplayName("Create plant - should add plant to database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreatePlantAsync_ShouldAddPlant()
        {
            var client = await CreateTestClientAsync();
            Plant plant = null!;
            
            // Создание растения с клиентом
            AllureApi.Step("Create plant with client", () => {
                plant = new PlantBuilder()
                    .WithClientId(client.Id)
                    .Build();
            });

            // Выполнение метода CreatePlantAsync
            await AllureApi.Step("Execute CreatePlantAsync", async () => {
                var result = await _repository.CreatePlantAsync(plant);
            });

            // Проверка создания растения в базе данных
            await AllureApi.Step("Verify plant created in database", async () => {
                var dbPlant = await _context.Plants.FirstOrDefaultAsync(p => p.Id == plant.Id);
                Assert.NotNull(dbPlant);
                Assert.Equal(plant.Specie, dbPlant.Specie);
                Assert.Equal(plant.Family, dbPlant.Family);
                Assert.Equal(client.Id, dbPlant.ClientId);
            });
        }

        [Fact]
        [DisplayName("Create plant - should throw exception for null input")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task CreatePlantAsync_ShouldThrowArgumentNullException()
        {
            var client = await CreateTestClientAsync();
            
            // Попытка создания с null растением
            await AllureApi.Step("Attempt to create with null plant", async () => {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _repository.CreatePlantAsync(null!));
            });
        }
        #endregion

        #region GetPlantByIdAsync Tests
        [Fact]
        [DisplayName("Get plant by ID - should return plant when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetPlantByIdAsync_ShouldReturnPlant_WhenExists()
        {
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            PlantDb plantDb = null!;
            
            // Настройка растения с ID
            await AllureApi.Step($"Setup plant with ID: {plantId}", async () => {
                plantDb = new PlantDbBuilder()
                    .WithId(plantId)
                    .WithClientId(client.Id)
                    .Build();
                
                await _context.Plants.AddAsync(plantDb);
                await _context.SaveChangesAsync();
            });

            // Выполнение метода GetPlantByIdAsync
            var result = await AllureApi.Step($"Execute GetPlantByIdAsync for ID: {plantId}", 
                async () => await _repository.GetPlantByIdAsync(plantId));

            // Проверка данных растения
            AllureApi.Step("Verify plant data", () => {
                Assert.Equal(plantId, result.Id);
                Assert.Equal(plantDb.Specie, result.Specie);
            });
        }

        [Fact]
        [DisplayName("Get plant by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantByIdAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();

            // Попытка получения несуществующего растения
            await AllureApi.Step($"Attempt to get non-existent plant with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<PlantNotFoundException>(
                    () => _repository.GetPlantByIdAsync(nonExistentId));
            });
        }
        #endregion

        #region GetAllPlantsAsync Tests
        [Fact]
        [DisplayName("Get all plants - should return all plants")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllPlantsAsync_ShouldReturnAllPlants()
        {
            var client = await CreateTestClientAsync();
            
            // Настройка нескольких растений
            await AllureApi.Step("Setup multiple plants", async () => {
                var plants = new List<PlantDb>
                {
                    new PlantDbBuilder().WithClientId(client.Id).Build(),
                    new PlantDbBuilder().WithClientId(client.Id).Build()
                };
                
                await _context.Plants.AddRangeAsync(plants);
                await _context.SaveChangesAsync();
            });

            // Выполнение метода GetAllPlantsAsync
            var result = await AllureApi.Step("Execute GetAllPlantsAsync", 
                async () => await _repository.GetAllPlantsAsync());

            // Проверка возврата 2 растений
            AllureApi.Step("Verify 2 plants returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion

        #region UpdatePlantAsync Tests
        [Fact]
        [DisplayName("Update plant - should update plant data")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task UpdatePlantAsync_ShouldUpdatePlant()
        {
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            Plant updatedPlant = null!;
            
            // Настройка исходного растения
            await AllureApi.Step($"Setup original plant with ID: {plantId}", async () => {
                var originalPlant = new PlantDbBuilder()
                    .WithId(plantId)
                    .WithClientId(client.Id)
                    .WithPlantSpecie("OldRose")
                    .WithPlantFamily("OldRosaceae")
                    .Build();
        
                await _context.Plants.AddAsync(originalPlant);
                await _context.SaveChangesAsync();
            });

            // Создание обновленного растения
            AllureApi.Step("Create updated plant", () => {
                updatedPlant = new PlantBuilder()
                    .WithId(plantId)
                    .WithClientId(client.Id)
                    .WithSpecie("NewRose")
                    .WithFamily("NewRosaceae")
                    .WithFlower(EnumFlowers.Zygomorphic)
                    .WithFruit(EnumFruit.Capsule)
                    .WithReproduction(EnumReproduction.Cutting)
                    .Build();
            });

            // Выполнение метода UpdatePlantAsync
            await AllureApi.Step($"Execute UpdatePlantAsync for ID: {plantId}", 
                async () => await _repository.UpdatePlantAsync(updatedPlant));

            // Проверка обновления растения
            await AllureApi.Step("Verify plant updated", async () => {
                var dbPlant = await _context.Plants.FindAsync(plantId);
                Assert.NotNull(dbPlant);
                Assert.Equal("NewRose", dbPlant!.Specie);
                Assert.Equal("NewRosaceae", dbPlant.Family);
                Assert.Equal(client.Id, dbPlant.ClientId);
                Assert.Equal(EnumFlowers.Zygomorphic, dbPlant.Flower);
                Assert.Equal(EnumFruit.Capsule, dbPlant.Fruit);
                Assert.Equal(EnumReproduction.Cutting, dbPlant.Reproduction);
            });
        }

        [Fact]
        [DisplayName("Update plant - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdatePlantAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            // Попытка обновления несуществующего растения
            await AllureApi.Step("Attempt to update non-existent plant", async () => {
                var nonExistentPlant = PlantMotherObject.CreateDefaultPlant();
                await Assert.ThrowsAsync<PlantNotFoundException>(
                    () => _repository.UpdatePlantAsync(nonExistentPlant));
            });
        }

        [Fact]
        [DisplayName("Update plant - should throw exception for null input")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task UpdatePlantAsync_ShouldThrowArgumentNullException()
        {
            // Попытка обновления с null растением
            await AllureApi.Step("Attempt to update with null plant", async () => {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _repository.UpdatePlantAsync(null!));
            });
        }
        #endregion

        #region DeletePlantAsync Tests
        [Fact]
        [DisplayName("Delete plant - should remove plant from database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task DeletePlantAsync_ShouldRemovePlant()
        {
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            
            // Настройка растения для удаления
            await AllureApi.Step($"Setup plant to delete with ID: {plantId}", async () => {
                var plant = new PlantDbBuilder()
                    .WithId(plantId)
                    .WithClientId(client.Id)
                    .Build();
                
                await _context.Plants.AddAsync(plant);
                await _context.SaveChangesAsync();
            });

            // Выполнение метода DeletePlantAsync
            await AllureApi.Step($"Execute DeletePlantAsync for ID: {plantId}", 
                async () => await _repository.DeletePlantAsync(plantId));

            // Проверка удаления растения
            await AllureApi.Step("Verify plant deleted", async () => {
                var dbPlant = await _context.Plants.FindAsync(plantId);
                Assert.Null(dbPlant);
            });
        }

        [Fact]
        [DisplayName("Delete plant - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task DeletePlantAsync_ShouldThrowNotFoundException_WhenNotExists()
        {
            var nonExistentId = Guid.NewGuid();

            // Попытка удаления несуществующего растения
            await AllureApi.Step($"Attempt to delete non-existent plant with ID: {nonExistentId}", async () => {
                await Assert.ThrowsAsync<PlantNotFoundException>(
                    () => _repository.DeletePlantAsync(nonExistentId));
            });
        }
        #endregion

        #region GetPlantsByFamilyAsync Tests
        [Fact]
        [DisplayName("Get plants by family - should return plants")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantsByFamilyAsync_ShouldReturnPlants()
        {
            var client = await CreateTestClientAsync();
            var family = "Rosaceae";
            
            // Настройка растений с семейством
            await AllureApi.Step($"Setup plants with family: {family}", async () => {
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
            });

            // Выполнение метода GetPlantsByFamilyAsync
            var result = await AllureApi.Step($"Execute GetPlantsByFamilyAsync for family: {family}", 
                async () => await _repository.GetPlantsByFamilyAsync(family));

            // Проверка возвращенных растений
            AllureApi.Step("Verify plants returned", () => {
                Assert.Equal(2, result.Count());
                Assert.All(result, p => Assert.Equal(family, p.Family));
            });
        }
        #endregion

        #region GetPlantsBySpeciesAsync Tests
        [Fact]
        [DisplayName("Get plants by species - should return plants")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantsBySpeciesAsync_ShouldReturnPlants()
        {
            var client = await CreateTestClientAsync();
            var species = "Rose";
            
            // Настройка растений с видом
            await AllureApi.Step($"Setup plants with species: {species}", async () => {
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
            });

            // Выполнение метода GetPlantsBySpeciesAsync
            var result = await AllureApi.Step($"Execute GetPlantsBySpeciesAsync for species: {species}", 
                async () => await _repository.GetPlantsBySpeciesAsync(species));

            // Проверка возвращенных растений
            AllureApi.Step("Verify plants returned", () => {
                Assert.Equal(2, result.Count());
                Assert.All(result, p => Assert.Equal(species, p.Specie));
            });
        }
        #endregion

        #region GetJournalRecordsByPlantIdAsync Tests
        [Fact]
        [DisplayName("Get journal records by plant ID - should return records")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetJournalRecordsByPlantIdAsync_ShouldReturnRecords()
        {
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            
            // Настройка растения и записей журнала
            await AllureApi.Step($"Setup plant and journal records with plant ID: {plantId}", async () => {
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
            });

            // Выполнение метода GetJournalRecordsByPlantIdAsync
            var result = await AllureApi.Step($"Execute GetJournalRecordsByPlantIdAsync for plant ID: {plantId}", 
                async () => await _repository.GetJournalRecordsByPlantIdAsync(plantId));

            // Проверка возвращенных записей журнала
            AllureApi.Step("Verify journal records returned", () => {
                Assert.Equal(2, result.Count());
                Assert.All(result, r => Assert.Equal(plantId, r.PlantId));
            });
        }
        #endregion

        #region GetSeedsByPlantIdAsync Tests
        [Fact]
        [DisplayName("Get seeds by plant ID - should return seeds")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetSeedsByPlantIdAsync_ShouldReturnSeeds()
        {
            var client = await CreateTestClientAsync();
            var plantId = Guid.NewGuid();
            
            // Настройка растения и семян
            await AllureApi.Step($"Setup plant and seeds with plant ID: {plantId}", async () => {
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
            });

            // Выполнение метода GetSeedsByPlantIdAsync
            var result = await AllureApi.Step($"Execute GetSeedsByPlantIdAsync for plant ID: {plantId}", 
                async () => await _repository.GetSeedsByPlantIdAsync(plantId));

            // Проверка возвращенных семян
            AllureApi.Step("Verify seeds returned", () => {
                Assert.Equal(2, result.Count());
                Assert.All(result, s => Assert.Equal(plantId, s.PlantId));
            });
        }
        #endregion

        #region GetPlantsByClientIdAsync Tests
        [Fact]
        [DisplayName("Get plants by client ID - should return plants")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantsByClientIdAsync_ShouldReturnPlants()
        {
            var client = await CreateTestClientAsync();
            
            // Настройка растений для клиента
            await AllureApi.Step($"Setup plants for client ID: {client.Id}", async () => {
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
            });

            // Выполнение метода GetPlantsByClientIdAsync
            var result = await AllureApi.Step($"Execute GetPlantsByClientIdAsync for client ID: {client.Id}", 
                async () => await _repository.GetPlantsByClientIdAsync(client.Id));

            // Проверка возвращенных растений
            AllureApi.Step("Verify plants returned", () => {
                Assert.Equal(2, result.Count());
                Assert.All(result, p => Assert.Equal(client.Id, p.ClientId));
            });
        }
        #endregion
    }
}