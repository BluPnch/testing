namespace DataAccess.Models;


public class ClientDb
{
    public Guid Id { get; protected set; }
    
    public string CompanyName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    
    
    public virtual ICollection<PlantDb> Plants { get; set; }
    
    /// <summary>
    /// Конструктор для создания экземпляра клиента.
    /// </summary>
    /// <param name="id">Идентификатор клиента.</param>
    /// <param name="companyName">Название компании клиента.</param>
    /// <param name="phoneNumber">Номер телефона клиента.</param>
    public ClientDb(Guid id, string companyName, string? phoneNumber)
    {
        Id = id;
        CompanyName = companyName;
        PhoneNumber = phoneNumber;
    }
}