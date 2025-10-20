using Domain.Models;

namespace Domain.Interfaces.Repositories;

public interface IEmployeeRepository
{
    /// <summary>
    /// Создать нового сотрудника.
    /// </summary>
    /// <param name="employee">Данные сотрудника.</param>
    /// <returns>Созданный сотрудник.</returns>
    Task<Employee> CreateEmployeeAsync(Employee employee);

    /// <summary>
    /// Получить всех сотрудников.
    /// </summary>
    /// <returns>Список всех сотрудников.</returns>
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    
    /// <summary>
    /// Получить сотрудника по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сотрудника.</param>
    /// <returns>Сотрудник с указанным идентификатором.</returns>
    Task<Employee> GetEmployeeByIdAsync(Guid id);

    /// <summary>
    /// Удалить сотрудника по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сотрудника.</param>
    /// <returns></returns>
    Task DeleteEmployeeAsync(Guid id);

    /// <summary>
    /// Получить сотрудников по их задаче.
    /// </summary>
    /// <param name="task">Задача сотрудника.</param>
    /// <returns>Список сотрудников с указанной задачей.</returns>
    Task<IEnumerable<Employee>> GetEmployeesByTaskAsync(string task);

    /// <summary>
    /// Получить сотрудников по сфере растений.
    /// </summary>
    /// <param name="plantDomain">Сфера растений.</param>
    /// <returns>Список сотрудников, работающих в указанной сфере растений.</returns>
    Task<IEnumerable<Employee>> GetEmployeesByPlantDomainAsync(string plantDomain);

    /// <summary>
    /// Получить сотрудника по номеру телефона.
    /// </summary>
    /// <param name="phoneNumber">Номер телефона.</param>
    /// <returns>Сотрудник с указанным номером телефона.</returns>
    Task<Employee> GetEmployeeByPhoneNumberAsync(string phoneNumber);
    
    /// <summary>
    /// Получить растения, с которыми работает сотрудник.
    /// </summary>
    /// <param name="employeeId">Идентификатор сотрудника.</param>
    /// <returns>Список растений.</returns>
    Task<IEnumerable<Plant>> GetPlantsByEmployeeIdAsync(Guid employeeId);
    
    /// <summary>
    /// Назначить сотрудника на растение.
    /// </summary>
    /// <param name="employeeId">Идентификатор сотрудника.</param>
    /// <param name="plantId">Идентификатор растения.</param>
    /// <returns></returns>
    Task AssignEmployeeToPlantAsync(Guid employeeId, Guid plantId);
}