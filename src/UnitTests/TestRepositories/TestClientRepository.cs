using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using DataAccess.Context;
using DataAccess.Models;
using DataAccess.Repositories;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UnitTests.Builders;
using UnitTests.ObjectMother;

namespace UnitTests.TestRepositories
{
    [AllureFeature("Client Management")]
    [AllureStory("Client Repository Operations")]
    public class TestClientRepository : IClassFixture<RepositoryTestFixture>
    {
        private readonly RepositoryTestFixture _fixture;
        private readonly GreenhouseContext _context;
        private readonly ClientRepository _repository;

        public TestClientRepository(RepositoryTestFixture fixture)
        {
            _fixture = fixture;
            _context = _fixture.Context;
            _repository = new ClientRepository(_context);
            
            ClearDatabaseAsync().Wait();
        }

        
        private async Task ClearDatabaseAsync()
        {
            await AllureApi.Step("Clear clients database", async () => {
                _context.Clients.RemoveRange(_context.Clients);
                await _context.SaveChangesAsync();
            });
        }
        

        #region CreateClient Tests
        [Fact]
        [AllureName("Create client - should add client to database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task CreateClientAsync_ShouldAddClientToDatabase()
        {
            await AllureApi.Step("Setup test data", async () => {
                var client = ClientObjectMother.CreateDefaultClient();
                
                await AllureApi.Step("Execute CreateClientAsync", async () => {
                    var result = await _repository.CreateClientAsync(client);
                });
            });

            await AllureApi.Step("Verify client created in database", async () => {
                var client = ClientObjectMother.CreateDefaultClient();
                var dbClient = await _context.Clients.FirstOrDefaultAsync(c => c.Id == client.Id);
                Assert.NotNull(dbClient);
                Assert.Equal(client.CompanyName, dbClient.CompanyName);
                Assert.Equal(client.PhoneNumber, dbClient.PhoneNumber);
            });
        }
        #endregion

        #region GetAllClients Tests
        [Fact]
        [AllureName("Get all clients - should return all clients")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            await AllureApi.Step("Setup test data with 2 clients", async () => {
                var clients = ClientObjectMother.CreateClientsList(2);
                await _context.Clients.AddRangeAsync(clients);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step("Execute GetAllClientsAsync", 
                async () => await _repository.GetAllClientsAsync());

            await AllureApi.Step("Verify 2 clients returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion

        #region GetClientById Tests
        [Fact]
        [AllureName("Get client by ID - should return client when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenExists()
        {
            var clientId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup test client with ID: {clientId}", async () => {
                var client = new ClientDbBuilder()
                    .WithId(clientId)
                    .WithCompanyName("Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
                
                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetClientByIdAsync for ID: {clientId}", 
                async () => await _repository.GetClientByIdAsync(clientId));

            await AllureApi.Step("Verify client data", () => {
                Assert.Equal(clientId, result.Id);
            });
        }

        [Fact]
        [AllureName("Get client by ID - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetClientByIdAsync_ShouldThrowClientNotFoundException_WhenNotExists()
        {
            var clientId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to get non-existent client with ID: {clientId}", async () => {
                var ex = await Assert.ThrowsAsync<ClientNotFoundException>(
                    () => _repository.GetClientByIdAsync(clientId));
                
                await AllureApi.Step("Verify exception type", () => {
                    Assert.IsType<ClientNotFoundException>(ex);
                });
            });
        }
        #endregion
        
        #region GetClientByCompanyName Tests
        [Fact]
        [AllureName("Get client by company name - should return client when exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task GetClientByCompanyNameAsync_ShouldReturnClient_WhenExists()
        {
            var companyName = "Test Company";
            
            await AllureApi.Step($"Setup test client with company name: {companyName}", async () => {
                var client = ClientObjectMother.CreateClientDbWithCompanyName(companyName);
                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetClientByCompanyNameAsync for: {companyName}", 
                async () => await _repository.GetClientByCompanyNameAsync(companyName));

            await AllureApi.Step("Verify company name matches", () => {
                Assert.Equal(companyName, result.CompanyName);
            });
        }

        [Fact]
        [AllureName("Get client by company name - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetClientByCompanyNameAsync_ShouldThrowClientNotFoundException_WhenNotExists()
        {
            var companyName = "Non-Existent Company";

            await AllureApi.Step($"Attempt to get client with non-existent company: {companyName}", async () => {
                await Assert.ThrowsAsync<ClientNotFoundException>(
                    () => _repository.GetClientByCompanyNameAsync(companyName));
            });
        }
        #endregion

        #region GetClientsByPhoneNumber Tests
        [Fact]
        [AllureName("Get clients by phone number - should return clients")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetClientsByPhoneNumberAsync_ShouldReturnClients()
        {
            var phoneNumber = "1234567890";
            
            await AllureApi.Step($"Setup 2 test clients with phone: {phoneNumber}", async () => {
                var clients = ClientObjectMother.CreateClientsList(2, phoneNumber);
                await _context.Clients.AddRangeAsync(clients);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetClientsByPhoneNumberAsync for: {phoneNumber}", 
                async () => await _repository.GetClientsByPhoneNumberAsync(phoneNumber));

            await AllureApi.Step("Verify 2 clients returned", () => {
                Assert.Equal(2, result.Count());
            });
        }

        [Fact]
        [AllureName("Get clients by phone number - should return empty list when no matches")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetClientsByPhoneNumberAsync_ShouldReturnEmptyList_WhenNoMatches()
        {
            var phoneNumber = "0000000000";

            var result = await AllureApi.Step($"Execute GetClientsByPhoneNumberAsync for non-existent phone: {phoneNumber}", 
                async () => await _repository.GetClientsByPhoneNumberAsync(phoneNumber));

            await AllureApi.Step("Verify empty list returned", () => {
                Assert.Empty(result);
            });
        }
        #endregion

        #region GetPlantsByClientId Tests
        [Fact]
        [AllureName("Get plants by client ID - should return plants")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task GetPlantsByClientIdAsync_ShouldReturnPlants()
        {
            var clientId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup client and plants with client ID: {clientId}", async () => {
                var client = new ClientDbBuilder()
                    .WithId(clientId)
                    .WithCompanyName("Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
        
                await _context.Clients.AddAsync(client);
                
                var plants = new List<PlantDb>
                {
                    new PlantDbBuilder()
                        .WithClientId(clientId)
                        .WithPlantSpecie("Specie1")
                        .Build(),
                        
                    new PlantDbBuilder()
                        .WithClientId(clientId)
                        .WithPlantSpecie("Specie2")
                        .Build()
                };
                
                await _context.Plants.AddRangeAsync(plants);
                await _context.SaveChangesAsync();
            });

            var result = await AllureApi.Step($"Execute GetPlantsByClientIdAsync for client ID: {clientId}", 
                async () => await _repository.GetPlantsByClientIdAsync(clientId));

            await AllureApi.Step("Verify 2 plants returned", () => {
                Assert.Equal(2, result.Count());
            });
        }
        #endregion
        
        #region DeleteClient Tests
        [Fact]
        [AllureName("Delete client - should remove client from database")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.critical)]
        public async Task DeleteClientAsync_ShouldRemoveClientFromDatabase()
        {
            var clientId = Guid.NewGuid();
            
            await AllureApi.Step($"Setup test client with ID: {clientId}", async () => {
                var client = new ClientDbBuilder()
                    .WithId(clientId)
                    .WithCompanyName("Company")
                    .WithPhoneNumber("1234567890")
                    .Build();
                
                await _context.Clients.AddAsync(client);
                await _context.SaveChangesAsync();
            });

            await AllureApi.Step($"Execute DeleteClientAsync for ID: {clientId}", 
                async () => await _repository.DeleteClientAsync(clientId));

            await AllureApi.Step("Verify client deleted from database", async () => {
                var dbClient = await _context.Clients.FindAsync(clientId);
                Assert.Null(dbClient);
            });
        }

        [Fact]
        [AllureName("Delete client - should throw exception when not exists")]
        [AllureOwner("Development Team")]
        [AllureSeverity(SeverityLevel.normal)]
        public async Task DeleteClientAsync_ShouldThrowClientNotFoundException_WhenNotExists()
        {
            var clientId = Guid.NewGuid();

            await AllureApi.Step($"Attempt to delete non-existent client with ID: {clientId}", async () => {
                await Assert.ThrowsAsync<ClientNotFoundException>(
                    () => _repository.DeleteClientAsync(clientId));
            });
        }
        #endregion
    }
}