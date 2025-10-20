namespace DataAccess.Models;


public class  AdministratorDb
{
    public Guid Id { get; protected set; }
    
    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronymic { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string Username { get; set; }
    
    
    
    public virtual ICollection<EmployeeDb> Employees { get; set; }
    
    /// <summary>
    /// Конструктор для создания экземпляра администратора.
    /// </summary>
    /// <param name="id">Идентификатор администратора.</param>
    /// <param name="surname">Фамилия администратора.</param>
    /// <param name="name">Имя администратора.</param>
    /// <param name="patronymic">Отчество администратора.</param>
    /// <param name="phoneNumber">Номер телефона администратора.</param>
    /// <param name="username">Логин администратора.</param>
    public AdministratorDb(Guid id, string? surname, string? name, string? patronymic, string? phoneNumber, string username) 
    {
        Id = id;
        Surname = surname;
        Name = name;
        Patronymic = patronymic;
        PhoneNumber = phoneNumber;
        Username = username;
    }
}