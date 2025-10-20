using Domain.Models;

namespace Domain.Interfaces.Services;

public interface IEmployeeService
{
    Task<Employee> CreateEmployeeAsync(Employee employee);

    Task<Employee> GetEmployeeByIdAsync(Guid employeeId);

    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    
    Task DeleteEmployeeAsync(Guid employeeId);
    
    Task AssignEmployeeToPlantAsync(Guid employeeId, Guid plantId);
    
    Task<IEnumerable<Plant>> GetPlantsByEmployeeIdAsync(Guid employeeId);

    Task<IEnumerable<Employee>> GetEmployeesByTaskAsync(string task);

    Task<IEnumerable<Employee>> GetEmployeesByPlantDomainAsync(string plantDomain);

    Task<Employee> GetEmployeeByPhoneNumberAsync(string phoneNumber);
}