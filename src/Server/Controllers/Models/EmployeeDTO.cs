namespace Server.Controllers.Models;

public class EmployeeDTO : BaseModelDTO
{
    public string? Surname { get; set; }
    public string? Name { get; set; }
    public string? Patronymic { get; set; }
    public string? PhoneNumber { get; set; }
    public string Task { get; set; }
    public string PlantDomain { get; set; }
    public Guid AdministratorId { get; set; }
}
