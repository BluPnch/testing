using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IClientRepository
{
    Task<Client> CreateClientAsync(Client client);
    
    Task DeleteClientAsync(Guid id);

    Task<IEnumerable<Client>> GetAllClientsAsync();
    
    Task<Client> GetClientByIdAsync(Guid id);

    Task<Client> GetClientByCompanyNameAsync(string companyName);

    Task<Client> GetClientByPhoneNumberAsync(string phoneNumber);

    Task<IEnumerable<Client>> GetClientsByPhoneNumberAsync(string phoneNumber);
    
    Task<IEnumerable<Plant>> GetPlantsByClientIdAsync(Guid clientId);
}