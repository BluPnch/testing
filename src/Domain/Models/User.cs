namespace Domain.Models;


public class User : BaseModel
{
    
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Конструктор для создания экземпляра пользователя.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    /// <param name="phoneNumber">Телефон пользователя.</param>
    protected User(Guid id, string? phoneNumber) : base(id)
    {
        PhoneNumber = phoneNumber;
    }
}