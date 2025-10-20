namespace Domain.Models;


public class Employee : Staff
{
    public string Task { get; set; }

    public string PlantDomain { get; set; }

    public Guid AdministratorId { get; set; }
    
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
    /// <param name="administratorId">Идентификатор администратора.</param>
    public Employee(Guid id, string? surname, string? name, string? patronymic, string task, string plantDomain, string? phoneNumber, Guid administratorId) : base(id, surname, name, patronymic, phoneNumber)
    {
        Task = task;
        PlantDomain = plantDomain;
        AdministratorId = administratorId;
    }
}