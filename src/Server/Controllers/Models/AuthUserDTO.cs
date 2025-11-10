namespace Server.Controllers.Models;

public class AuthUserDTO
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public Enums.EnumAuth Role { get; set; }
}
