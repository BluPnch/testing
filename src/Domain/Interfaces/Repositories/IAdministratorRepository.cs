using Domain.Models;


namespace Domain.Interfaces.Repositories;

public interface IAdministratorRepository
{
    Task<IEnumerable<Administrator>> GetAllAdministratorsAsync();
    Task<Administrator> GetAdministratorByIdAsync(Guid id);
    Task<Administrator> GetAdministratorByPhoneNumberAsync(string phoneNumber);
    Task<Administrator> GetAdministratorByFullNameAsync(string surname, string name, string patronymic);
    Task<Administrator> CreateAdministratorAsync(Guid id, string phoneNumber, string surname, string name, string? patronymic, string username);
    // Task<Administrator> UpdateAdministratorAsync(Administrator administrator);
}