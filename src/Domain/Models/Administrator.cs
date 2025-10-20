namespace Domain.Models;


public class  Administrator : Staff
{
    public string Username { get; set; }

    /// <summary>
    /// Конструктор для создания экземпляра администратора.
    /// </summary>
    /// <param name="id">Идентификатор администратора.</param>
    /// <param name="surname">Фамилия администратора.</param>
    /// <param name="name">Имя администратора.</param>
    /// <param name="patronymic">Отчество администратора.</param>
    /// <param name="phoneNumber">Номер телефона администратора.</param>
    /// <param name="username">Логин администратора.</param>
    public Administrator(Guid id, string? surname, string? name, string? patronymic, string? phoneNumber, string username) 
        : base(id, surname, name, patronymic, phoneNumber)
    {
        Username = username;
    }
}