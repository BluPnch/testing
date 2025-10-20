using Domain.Interfaces.Repositories;
using Domain.Models;
using DataAccess.Context;
using DataAccess.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Domain.Exceptions;

namespace DataAccess.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly GreenhouseContext _context;

        public ClientRepository(GreenhouseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            var clientDb = client.ToDb();

            await _context.Clients.AddAsync(clientDb!);
            await _context.SaveChangesAsync();

            return clientDb.ToDomain()!;
        }
        
        
        public async Task DeleteClientAsync(Guid id)
        {
            var client = await _context.Clients.FindAsync(id);
            
            if (client == null)
                throw new ClientNotFoundException($"Client with id '{id}' not found");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Client>> GetAllClientsAsync()
        {
            var clients = await _context.Clients
                .AsNoTracking()
                .ToListAsync();

            return clients.ToDomain();
        }

        public async Task<Client> GetClientByIdAsync(Guid id)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                throw new ClientNotFoundException($"Client with id '{id}' not found");

            return client.ToDomain()!;
        }

        public async Task<Client> GetClientByCompanyNameAsync(string companyName)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyName == companyName);

            if (client == null)
                throw new ClientNotFoundException($"Client with company name '{companyName}' not found");

            return client.ToDomain()!;
        }

        public async Task<IEnumerable<Client>> GetClientsByPhoneNumberAsync(string phoneNumber)
        {
            var clients = await _context.Clients
                .AsNoTracking()
                .Where(c => c.PhoneNumber == phoneNumber)
                .ToListAsync();

            return clients.ToDomain();
        }

        public async Task<Client> GetClientByPhoneNumberAsync(string phoneNumber)
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

            if (client == null)
                throw new ClientNotFoundException($"Client with phone number '{phoneNumber}' not found");

            return client.ToDomain()!;
        }

        public async Task<IEnumerable<Plant>> GetPlantsByClientIdAsync(Guid clientId)
        {
            var plants = await _context.Plants
                .AsNoTracking()
                .Where(p => p.ClientId == clientId)
                .ToListAsync();

            return plants.ToDomain();
        }
    }
}