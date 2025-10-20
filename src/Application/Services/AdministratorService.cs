using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using Domain.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class AdministratorService : IAdministratorService
{
    private readonly IAdministratorRepository _administratorRepository;
    private readonly AdministratorValidator _administratorValidator;
    private readonly IAuthUserRepository _authUserRepository;
    private readonly ILogger<AdministratorService> _logger;

    public AdministratorService(
        IAdministratorRepository administratorRepository, 
        AdministratorValidator administratorValidator,
        IAuthUserRepository authUserRepository,
        ILogger<AdministratorService> logger,
        IConfiguration configuration)
    {
        _administratorRepository = administratorRepository ?? 
                                   throw new ArgumentNullException(nameof(administratorRepository));
        _administratorValidator = administratorValidator ?? 
                                  throw new ArgumentNullException(nameof(administratorValidator));
        _authUserRepository = authUserRepository ?? 
                             throw new ArgumentNullException(nameof(authUserRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Administrator> CreateAdministratorAsync(Guid id, string phoneNumber, string surname, string name, string patronymic, string username, string password)
    {
        _logger.LogInformation("Attempting to create administrator with username: {Username}", username);

        var existingAdmin = await _administratorRepository.GetAdministratorByPhoneNumberAsync(phoneNumber);
        if (existingAdmin != null)
        {
            _logger.LogWarning("Administrator with phone number {PhoneNumber} already exists", phoneNumber);
            throw new InvalidOperationException("Администратор с таким номером телефона уже существует.");
        }
        
        if (await _authUserRepository.UsernameExistsAsync(username))
        {
            _logger.LogWarning("User with username {Username} already exists", username);
            throw new InvalidOperationException("Пользователь с таким логином уже существует.");
        }

        try
        {
            var authUser = new AuthUser
            {
                Id = id,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = EnumAuth.Administrator
            };

            await _authUserRepository.CreateAsync(authUser);
            _logger.LogInformation("Created auth user for administrator with username: {Username}", username);

            var administrator = await _administratorRepository.CreateAdministratorAsync(id, phoneNumber, surname, name, patronymic, username);
            _logger.LogInformation("Successfully created administrator with ID: {AdministratorId}", id);
            
            return administrator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating administrator with username: {Username}", username);
            throw;
        }
    }

    public async Task<IEnumerable<Administrator>> GetAllAdministratorsAsync()
    {
        _logger.LogInformation("Retrieving all administrators");
        try
        {
            var administrators = await _administratorRepository.GetAllAdministratorsAsync();
            _logger.LogInformation("Successfully retrieved {Count} administrators", administrators.Count());
            return administrators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while retrieving administrators");
            throw;
        }
    }

    public async Task<Administrator> GetAdministratorByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving administrator with ID: {AdministratorId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve administrator with empty ID");
            throw new ArgumentException("Administrator Id cannot be empty", nameof(id));
        }

        try
        {
            var administrator = await _administratorRepository.GetAdministratorByIdAsync(id);
            _logger.LogInformation("Successfully retrieved administrator with ID: {AdministratorId}", id);
            return administrator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving administrator with ID: {AdministratorId}", id);
            throw;
        }
    }

    public async Task<Administrator> GetAdministratorByPhoneNumberAsync(string phoneNumber)
    {
        _logger.LogInformation("Retrieving administrator with phone number: {PhoneNumber}", phoneNumber);
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Attempted to retrieve administrator with empty phone number");
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        }

        try
        {
            var administrator = await _administratorRepository.GetAdministratorByPhoneNumberAsync(phoneNumber);
            _logger.LogInformation("Successfully retrieved administrator with phone number: {PhoneNumber}", phoneNumber);
            return administrator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving administrator with phone number: {PhoneNumber}", phoneNumber);
            throw;
        }
    }

    public async Task<Administrator> GetAdministratorByFullNameAsync(
        string surname, string name, string patronymic)
    {
        _logger.LogInformation("Retrieving administrator with name: {Surname} {Name} {Patronymic}", 
            surname, name, patronymic);
        
        if (string.IsNullOrWhiteSpace(surname))
        {
            _logger.LogWarning("Attempted to retrieve administrator with empty surname");
            throw new ArgumentException("Surname cannot be empty", nameof(surname));
        }
        
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempted to retrieve administrator with empty name");
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }

        try
        {
            var administrator = await _administratorRepository.GetAdministratorByFullNameAsync(
                surname, name, patronymic);
            _logger.LogInformation("Successfully retrieved administrator with name: {Surname} {Name} {Patronymic}", 
                surname, name, patronymic);
            return administrator;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving administrator with name: {Surname} {Name} {Patronymic}", 
                surname, name, patronymic);
            throw;
        }
    }
}