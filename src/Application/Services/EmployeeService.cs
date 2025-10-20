using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPlantRepository _plantRepository;
    private readonly EmployeeValidator _employeeValidator;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IPlantRepository plantRepository,
        EmployeeValidator employeeValidator,
        ILogger<EmployeeService> logger,
        IConfiguration configuration)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _employeeValidator = employeeValidator ?? throw new ArgumentNullException(nameof(employeeValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        if (employee == null)
        {
            _logger.LogWarning("Attempted to create null employee");
            throw new ArgumentNullException(nameof(employee));
        }

        _logger.LogInformation("Attempting to create employee with ID: {EmployeeId}", employee.Id);
        try
        {
            await _employeeValidator.ValidateAndThrowAsync(employee);
            var result = await _employeeRepository.CreateEmployeeAsync(employee);
            _logger.LogInformation("Successfully created employee with ID: {EmployeeId}", employee.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee with ID: {EmployeeId}", employee.Id);
            throw;
        }
    }

    public async Task<Employee> GetEmployeeByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving employee with ID: {EmployeeId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve employee with empty ID");
            throw new ArgumentException("Employee Id cannot be empty", nameof(id));
        }

        try
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            _logger.LogInformation("Successfully retrieved employee with ID: {EmployeeId}", id);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with ID: {EmployeeId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        _logger.LogInformation("Retrieving all employees");
        try
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            _logger.LogInformation("Successfully retrieved {Count} employees", employees.Count());
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees");
            throw new ApplicationException("Failed to retrieve employees", ex);
        }
    }

    public async Task DeleteEmployeeAsync(Guid id)
    {
        _logger.LogInformation("Attempting to delete employee with ID: {EmployeeId}", id);
        
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to delete employee with empty ID");
            throw new ArgumentException("Employee Id cannot be empty", nameof(id));
        }

        try
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
                throw new KeyNotFoundException($"Employee with ID {id} not found");
            }

            await _employeeRepository.DeleteEmployeeAsync(id);
            _logger.LogInformation("Successfully deleted employee with ID: {EmployeeId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee with ID: {EmployeeId}", id);
            throw;
        }
    }

    public async Task AssignEmployeeToPlantAsync(Guid employeeId, Guid plantId)
    {
        _logger.LogInformation("Attempting to assign employee {EmployeeId} to plant {PlantId}", 
            employeeId, plantId);
        
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to assign employee with empty ID");
            throw new ArgumentException("Employee Id cannot be empty", nameof(employeeId));
        }

        if (plantId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to assign to plant with empty ID");
            throw new ArgumentException("Plant Id cannot be empty", nameof(plantId));
        }

        try
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found", employeeId);
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
            }

            var plant = await _plantRepository.GetPlantByIdAsync(plantId);
            if (plant == null)
            {
                _logger.LogWarning("Plant with ID {PlantId} not found", plantId);
                throw new KeyNotFoundException($"Plant with ID {plantId} not found");
            }

            await _employeeRepository.AssignEmployeeToPlantAsync(employeeId, plantId);
            _logger.LogInformation("Successfully assigned employee {EmployeeId} to plant {PlantId}", 
                employeeId, plantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning employee {EmployeeId} to plant {PlantId}", 
                employeeId, plantId);
            throw;
        }
    }

    public async Task<IEnumerable<Plant>> GetPlantsByEmployeeIdAsync(Guid employeeId)
    {
        _logger.LogInformation("Retrieving plants for employee with ID: {EmployeeId}", employeeId);
        
        if (employeeId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve plants for employee with empty ID");
            throw new ArgumentException("Employee Id cannot be empty", nameof(employeeId));
        }

        try
        {
            var plants = await _employeeRepository.GetPlantsByEmployeeIdAsync(employeeId);
            _logger.LogInformation("Successfully retrieved {Count} plants for employee with ID: {EmployeeId}", 
                plants.Count(), employeeId);
            return plants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving plants for employee with ID: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByTaskAsync(string task)
    {
        _logger.LogInformation("Retrieving employees with task: {Task}", task);
        
        if (string.IsNullOrWhiteSpace(task))
        {
            _logger.LogWarning("Attempted to retrieve employees with empty task");
            throw new ArgumentException("Task cannot be empty", nameof(task));
        }

        try
        {
            var employees = await _employeeRepository.GetEmployeesByTaskAsync(task);
            _logger.LogInformation("Successfully retrieved {Count} employees with task: {Task}", 
                employees.Count(), task);
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees with task: {Task}", task);
            throw;
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByPlantDomainAsync(string plantDomain)
    {
        _logger.LogInformation("Retrieving employees with plant domain: {Domain}", plantDomain);
        
        if (string.IsNullOrWhiteSpace(plantDomain))
        {
            _logger.LogWarning("Attempted to retrieve employees with empty plant domain");
            throw new ArgumentException("Plant domain cannot be empty", nameof(plantDomain));
        }

        try
        {
            var employees = await _employeeRepository.GetEmployeesByPlantDomainAsync(plantDomain);
            _logger.LogInformation("Successfully retrieved {Count} employees with plant domain: {Domain}", 
                employees.Count(), plantDomain);
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees with plant domain: {Domain}", plantDomain);
            throw;
        }
    }

    public async Task<Employee> GetEmployeeByPhoneNumberAsync(string phoneNumber)
    {
        _logger.LogInformation("Retrieving employee with phone number: {PhoneNumber}", phoneNumber);
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Attempted to retrieve employee with empty phone number");
            throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        }

        try
        {
            var employee = await _employeeRepository.GetEmployeeByPhoneNumberAsync(phoneNumber);
            _logger.LogInformation("Successfully retrieved employee with phone number: {PhoneNumber}", phoneNumber);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee with phone number: {PhoneNumber}", phoneNumber);
            throw;
        }
    }
}