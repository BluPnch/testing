namespace Domain.Models;


public class Staff : User
{
    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronymic { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра пользователя.
    /// </summary>
    /// <param name="surname">Фамилия пользователя.</param>
    /// <param name="name">Имя пользователя.</param>
    /// <param name="patronymic">Отчество пользователя.</param>
    protected Staff(Guid id, string? surname, string? name, string? patronymic, string? phoneNumber) : base(id, phoneNumber)
    {
        Surname = surname;
        Name = name;
        Patronymic = patronymic;
    }
}