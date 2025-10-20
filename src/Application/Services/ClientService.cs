using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IPlantRepository _plantRepository;
    private readonly IJournalRecordRepository _journalRecordRepository;
    private readonly IAuthUserRepository _authUserRepository;
    private readonly ClientValidator _clientValidator;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clientRepository,
        IPlantRepository plantRepository,
        IJournalRecordRepository journalRecordRepository,
        IAuthUserRepository authUserRepository,
        ClientValidator clientValidator,
        ILogger<ClientService> logger,
        IConfiguration configuration)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _journalRecordRepository = journalRecordRepository ?? throw new ArgumentNullException(nameof(journalRecordRepository));
        _authUserRepository = authUserRepository ?? throw new ArgumentNullException(nameof(authUserRepository));
        _clientValidator = clientValidator ?? throw new ArgumentNullException(nameof(clientValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Client> CreateClientAsync(Client client)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));

        _logger.LogInformation("Попытка создания клиента с ID: {ClientId}", client.Id);
        try
        {
            await _clientValidator.ValidateAndThrowAsync(client);
            var result = await _clientRepository.CreateClientAsync(client);
            _logger.LogInformation("Клиент успешно создан с ID: {ClientId}", client.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании клиента с ID: {ClientId}", client.Id);
            throw;
        }
    }

    public async Task<IEnumerable<Client>> GetAllClientsAsync()
    {
        _logger.LogInformation("Retrieving all clients");
        try
        {
            var clients = await _clientRepository.GetAllClientsAsync();
            _logger.LogInformation("Successfully retrieved {Count} clients", clients.Count());
            return clients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all clients");
            throw new ApplicationException("Failed to retrieve clients", ex);
        }
    }

    public async Task<Client> GetClientByIdAsync(Guid id)
    {
        _logger.LogInformation("Получение клиента по ID: {ClientId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения клиента с пустым ID");
            throw new ArgumentException("ID клиента не может быть пустым", nameof(id));
        }

        try
        {
            var client = await _clientRepository.GetClientByIdAsync(id);
            _logger.LogInformation("Успешно получен клиент с ID: {ClientId}", id);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении клиента с ID: {ClientId}", id);
            throw;
        }
    }

    public async Task DeleteClientAsync(Guid id)
    {
        _logger.LogInformation("Попытка удаления клиента с ID: {ClientId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Попытка удаления клиента с пустым ID");
            throw new ArgumentException("ID клиента не может быть пустым", nameof(id));
        }

        try
        {
            var client = await _clientRepository.GetClientByIdAsync(id);
            if (client == null)
            {
                _logger.LogWarning("Клиент с ID {ClientId} не найден", id);
                throw new KeyNotFoundException($"Клиент с ID {id} не найден");
            }

            await _clientRepository.DeleteClientAsync(id);
            _logger.LogInformation("Клиент с ID {ClientId} успешно удален", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении клиента с ID: {ClientId}", id);
            throw;
        }
    }

    public async Task<Client> GetClientByCompanyNameAsync(string companyName)
    {
        _logger.LogInformation("Получение клиента по названию компании: {CompanyName}", companyName);
        
        if (string.IsNullOrWhiteSpace(companyName))
        {
            _logger.LogWarning("Попытка получения клиента с пустым названием компании");
            throw new ArgumentException("Название компании не может быть пустым", nameof(companyName));
        }

        try
        {
            var client = await _clientRepository.GetClientByCompanyNameAsync(companyName);
            _logger.LogInformation("Успешно получен клиент с названием компании: {CompanyName}", companyName);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении клиента с названием компании: {CompanyName}", companyName);
            throw;
        }
    }

    public async Task<IEnumerable<Client>> GetClientsByPhoneNumberAsync(string phoneNumber)
    {
        _logger.LogInformation("Получение клиентов по номеру телефона: {PhoneNumber}", phoneNumber);
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Попытка получения клиентов с пустым номером телефона");
            throw new ArgumentException("Номер телефона не может быть пустым", nameof(phoneNumber));
        }

        try
        {
            var clients = await _clientRepository.GetClientsByPhoneNumberAsync(phoneNumber);
            _logger.LogInformation("Успешно получено {Count} клиентов с номером телефона: {PhoneNumber}", 
                clients.Count(), phoneNumber);
            return clients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении клиентов с номером телефона: {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task<IEnumerable<Plant>> GetPlantsByClientIdAsync(Guid clientId)
    {
        _logger.LogInformation("Получение растений клиента с ID: {ClientId}", clientId);
        
        if (clientId == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения растений клиента с пустым ID");
            throw new ArgumentException("ID клиента не может быть пустым", nameof(clientId));
        }

        try
        {
            var plants = await _clientRepository.GetPlantsByClientIdAsync(clientId);
            _logger.LogInformation("Успешно получено {Count} растений для клиента с ID: {ClientId}", 
                plants.Count(), clientId);
            return plants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении растений клиента с ID: {ClientId}", clientId);
            _logger.LogError($"Ошибка при получении растений клиента с ID: {clientId}", ex);
            throw;
        }
    }

    public async Task<IEnumerable<Plant>> GetClientPlantsAsync(Guid clientId)
    {
        _logger.LogInformation("Получение растений клиента с ID: {ClientId}", clientId);
        
        if (clientId == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения растений клиента с пустым ID");
            throw new ArgumentException("ID клиента не может быть пустым", nameof(clientId));
        }

        try
        {
            var plants = await _plantRepository.GetPlantsByClientIdAsync(clientId);
            _logger.LogInformation("Успешно получено {Count} растений для клиента с ID: {ClientId}", 
                plants.Count(), clientId);
            return plants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении растений клиента с ID: {ClientId}", clientId);
            throw;
        }
    }

    public async Task<IEnumerable<JournalRecord>> GetClientJournalRecordsAsync(Guid clientId)
    {
        _logger.LogInformation("Получение записей журнала для клиента с ID: {ClientId}", clientId);
        
        if (clientId == Guid.Empty)
        {
            _logger.LogWarning("Попытка получения записей журнала для клиента с пустым ID");
            throw new ArgumentException("ID клиента не может быть пустым", nameof(clientId));
        }

        try
        {
            // Получаем все растения клиента
            var plants = await _plantRepository.GetPlantsByClientIdAsync(clientId);
            var plantIds = plants.Select(p => p.Id).ToList();

            // Получаем все записи журнала для этих растений
            var records = new List<JournalRecord>();
            foreach (var plantId in plantIds)
            {
                var plantRecords = await _journalRecordRepository.GetJournalRecordsByPlantIdAsync(plantId);
                records.AddRange(plantRecords);
            }

            _logger.LogInformation("Успешно получено {Count} записей журнала для клиента с ID: {ClientId}", 
                records.Count, clientId);
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записей журнала для клиента с ID: {ClientId}", clientId);
            throw;
        }
    }

    public async Task<Client?> GetClientByUsernameAsync(string username)
    {
        _logger.LogInformation("Attempting to get client by username: {Username}", username);

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogWarning("Username is null or empty");
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        }

        try
        {
            var authUser = await _authUserRepository.GetByUsernameAsync(username);
            if (authUser == null)
            {
                _logger.LogWarning("Auth user with username {Username} not found", username);
                return null;
            }

            var client = await _clientRepository.GetClientByIdAsync(authUser.Id);
            if (client == null)
            {
                _logger.LogWarning("Client with id {ClientId} not found", authUser.Id);
                return null;
            }

            _logger.LogInformation("Successfully retrieved client with username {Username}", username);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client by username: {Username}", username);
            throw;
        }
    }

    public async Task<Client> GetClientByPhoneNumberAsync(string phoneNumber)
    {
        _logger.LogInformation("Получение клиента по номеру телефона: {PhoneNumber}", phoneNumber);
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Попытка получения клиента с пустым номером телефона");
            throw new ArgumentException("Номер телефона не может быть пустым", nameof(phoneNumber));
        }

        try
        {
            var client = await _clientRepository.GetClientByPhoneNumberAsync(phoneNumber);
            _logger.LogInformation("Успешно получен клиент с номером телефона: {PhoneNumber}", phoneNumber);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении клиента с номером телефона: {PhoneNumber}", phoneNumber);
            throw;
        }
    }
}