using Domain.Models;
using Server.Controllers.Models;

namespace Server.Controllers.Converters;

public static class EmployeeConverter
{
    public static EmployeeDTO ToDTO(Employee employee)
    {
        if (employee == null) return null;
        
        return new EmployeeDTO
        {
            Id = employee.Id,
            Surname = employee.Surname,
            Name = employee.Name,
            Patronymic = employee.Patronymic,
            PhoneNumber = employee.PhoneNumber,
            Task = employee.Task,
            PlantDomain = employee.PlantDomain,
            AdministratorId = employee.AdministratorId
        };
    }

    public static Employee ToDomain(EmployeeDTO dto)
    {
        if (dto == null) return null;
        
        return new Employee(dto.Id, dto.Surname, dto.Name, dto.Patronymic, dto.Task, dto.PlantDomain, dto.PhoneNumber, dto.AdministratorId);
    }

    public static IEnumerable<EmployeeDTO> ToDTO(IEnumerable<Employee> employees)
    {
        return employees?.Select(ToDTO) ?? Enumerable.Empty<EmployeeDTO>();
    }
}

