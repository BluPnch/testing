namespace Domain.Models;


public class Client : User
{
    public string CompanyName { get; set; }
    
    /// <summary>
    /// Конструктор для создания экземпляра клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <param name="companyName">Название компании клиента.</param>
    /// <param name="phoneNumber">Номер телефона клиента.</param>
    public Client(Guid id, string companyName, string? phoneNumber) : base(id, phoneNumber)
    {
        CompanyName = companyName;
    }
}