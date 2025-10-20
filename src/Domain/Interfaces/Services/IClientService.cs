using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IClientService
{
    Task<Client> CreateClientAsync(Client client);

    Task<IEnumerable<Client>> GetAllClientsAsync();

    Task<Client> GetClientByIdAsync(Guid id);

    Task<Client?> GetClientByUsernameAsync(string username);

    Task DeleteClientAsync(Guid id);

    Task<Client> GetClientByCompanyNameAsync(string companyName);

    Task<Client> GetClientByPhoneNumberAsync(string phoneNumber);

    Task<IEnumerable<Plant>> GetClientPlantsAsync(Guid clientId);

    Task<IEnumerable<JournalRecord>> GetClientJournalRecordsAsync(Guid clientId);
}