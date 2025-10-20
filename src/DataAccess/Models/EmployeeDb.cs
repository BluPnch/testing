namespace DataAccess.Models;


public class EmployeeDb
{
    public Guid Id { get; protected set; }
    
    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronymic { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string Task { get; set; }

    public string PlantDomain { get; set; }
    
    
    
    public Guid AdministratorId { get; set; }
    
    public virtual AdministratorDb Administrator { get; set; }

    public virtual ICollection<EmployeePlantDb> EmployeePlants { get; set; } = [];
    
    public virtual ICollection<JournalRecordDb> JournalRecords { get; protected set; } = [];
    
    /// <summary>
    /// Конструктор для создания экземпляра сотрудника.
    /// </summary>
    /// <param name="id">Идентификатор сотрудника.</param>
    /// <param name="surname">Фамилия сотрудника.</param>
    /// <param name="name">Имя сотрудника.</param>
    /// <param name="patronymic">Отчество сотрудника.</param>
    /// <param name="task">Задача сотрудника.</param>
    /// <param name="plantDomain">Сфера растений, за которую отвечает сотрудник.</param>
    /// <param name="phoneNumber">Номер телефона сотрудника.</param>
    public EmployeeDb(Guid id, string? surname, string? name, string? patronymic, string task, string plantDomain, string? phoneNumber)
    {
        Id = id;
        Surname = surname;
        Name = name;
        Patronymic = patronymic;
        Task = task;
        PlantDomain = plantDomain;
        PhoneNumber = phoneNumber;
    }
}