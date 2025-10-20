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
            _context.Clients.RemoveRange(_context.Clients);
            await _context.SaveChangesAsync();
        }
        

        #region CreateClient Tests
        [Fact]
        public async Task CreateClientAsync_ShouldAddClientToDatabase()
        {
            
            var client = ClientObjectMother.CreateDefaultClient();

            
            var result = await _repository.CreateClientAsync(client);

            // Assert
            var dbClient = await _context.Clients.FirstOrDefaultAsync(c => c.Id == client.Id);
            Assert.NotNull(dbClient);
            Assert.Equal(client.CompanyName, dbClient.CompanyName);
            Assert.Equal(client.PhoneNumber, dbClient.PhoneNumber);
        }
        #endregion

        #region GetAllClients Tests
        [Fact]
        public async Task GetAllClientsAsync_ShouldReturnAllClients()
        {
            
            var clients = ClientObjectMother.CreateClientsList(2);
            
            await _context.Clients.AddRangeAsync(clients);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetAllClientsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region GetClientById Tests
        [Fact]
        public async Task GetClientByIdAsync_ShouldReturnClient_WhenExists()
        {
            
            var clientId = Guid.NewGuid();
            var client = new ClientDbBuilder()
                .WithId(clientId)
                .WithCompanyName("Company")
                .WithPhoneNumber("1234567890")
                .Build();
            
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetClientByIdAsync(clientId);

            // Assert
            Assert.Equal(clientId, result.Id);
        }

        [Fact]
        public async Task GetClientByIdAsync_ShouldThrowClientNotFoundException_WhenNotExists()
        {
            
            var clientId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<ClientNotFoundException>(
                () => _repository.GetClientByIdAsync(clientId));
        }
        #endregion
        
        #region GetClientByCompanyName Tests
        [Fact]
        public async Task GetClientByCompanyNameAsync_ShouldReturnClient_WhenExists()
        {
            
            var companyName = "Test Company";
            var client = ClientObjectMother.CreateClientDbWithCompanyName(companyName);
            
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetClientByCompanyNameAsync(companyName);

            // Assert
            Assert.Equal(companyName, result.CompanyName);
        }

        [Fact]
        public async Task GetClientByCompanyNameAsync_ShouldThrowClientNotFoundException_WhenNotExists()
        {
            
            var companyName = "Non-Existent Company";

            
            await Assert.ThrowsAsync<ClientNotFoundException>(
                () => _repository.GetClientByCompanyNameAsync(companyName));
        }
        #endregion

        #region GetClientsByPhoneNumber Tests
        [Fact]
        public async Task GetClientsByPhoneNumberAsync_ShouldReturnClients()
        {
            
            var phoneNumber = "1234567890";
            var clients = ClientObjectMother.CreateClientsList(2, phoneNumber);
            
            await _context.Clients.AddRangeAsync(clients);
            await _context.SaveChangesAsync();

            
            var result = await _repository.GetClientsByPhoneNumberAsync(phoneNumber);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetClientsByPhoneNumberAsync_ShouldReturnEmptyList_WhenNoMatches()
        {
            
            var phoneNumber = "0000000000";

            
            var result = await _repository.GetClientsByPhoneNumberAsync(phoneNumber);

            // Assert
            Assert.Empty(result);
        }
        #endregion

        #region GetPlantsByClientId Tests
        [Fact]
        public async Task GetPlantsByClientIdAsync_ShouldReturnPlants()
        {
            
            var clientId = Guid.NewGuid();
            var client = new ClientDbBuilder()
                .WithId(clientId)
                .WithCompanyName("Company")
                .WithPhoneNumber("1234567890")
                .Build();
    
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();
            
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

            
            var result = await _repository.GetPlantsByClientIdAsync(clientId);

            // Assert
            Assert.Equal(2, result.Count());
        }
        #endregion
        
        #region DeleteClient Tests
        [Fact]
        public async Task DeleteClientAsync_ShouldRemoveClientFromDatabase()
        {
            
            var clientId = Guid.NewGuid();
            var client = new ClientDbBuilder()
                .WithId(clientId)
                .WithCompanyName("Company")
                .WithPhoneNumber("1234567890")
                .Build();
            
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            
            await _repository.DeleteClientAsync(clientId);

            // Assert
            var dbClient = await _context.Clients.FindAsync(clientId);
            Assert.Null(dbClient);
        }

        [Fact]
        public async Task DeleteClientAsync_ShouldThrowClientNotFoundException_WhenNotExists()
        {
            
            var clientId = Guid.NewGuid();

            
            await Assert.ThrowsAsync<ClientNotFoundException>(
                () => _repository.DeleteClientAsync(clientId));
        }
        #endregion
    }
}