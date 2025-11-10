namespace Server.Controllers.Models;

public class AdministratorDTO : BaseModelDTO
{
    public string? Surname { get; set; }
    public string? Name { get; set; }
    public string? Patronymic { get; set; }
    public string? PhoneNumber { get; set; }
    public string Username { get; set; }
}
