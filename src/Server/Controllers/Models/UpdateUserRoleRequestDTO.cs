using Domain.Models.Enums;


namespace Server.Controllers.Models;

public class UpdateUserRoleRequestDto
{
    public EnumAuth NewRole { get; set; }
}