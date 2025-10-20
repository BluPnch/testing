using Domain.Models;


namespace DataAccess.Models.Converters;


public static class EmployeeConverter
{
    public static Employee? ToDomain(this EmployeeDb? employee)
    {
        if (employee is null) return null;

        return new Employee(
            id: employee.Id,
            surname: employee.Surname,
            name: employee.Name,
            patronymic: employee.Patronymic,
            task: employee.Task, 
            plantDomain: employee.PlantDomain,
            phoneNumber: employee.PhoneNumber,
            administratorId: employee.AdministratorId
        );
    }

    public static EmployeeDb? ToDb(this Employee? employee)
    {
        if (employee is null) return null;

        return new EmployeeDb(
            id: employee.Id,
            surname: employee.Surname,
            name: employee.Name,
            patronymic: employee.Patronymic,
            task: employee.Task, 
            plantDomain: employee.PlantDomain,
            phoneNumber: employee.PhoneNumber
        )
        {
            Task = employee.Task,
            PlantDomain = employee.PlantDomain,
            AdministratorId = employee.AdministratorId
        };
    }

    public static IEnumerable<Employee> ToDomain(this IEnumerable<EmployeeDb> employees)
        => employees.Select(e => e.ToDomain())!;

    public static IEnumerable<EmployeeDb> ToDb(this IEnumerable<Employee> employees)
        => employees.Select(e => e.ToDb())!;
}