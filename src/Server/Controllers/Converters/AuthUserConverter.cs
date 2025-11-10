using Domain.Models;
using Domain.Models.Enums;
using Server.Controllers.Models;
using EnumAuthDTO = Server.Controllers.Models.Enums.EnumAuth;

namespace Server.Controllers.Converters;

public static class AuthUserConverter
{
    public static AuthUserDTO ToDTO(AuthUser authUser)
    {
        if (authUser == null) return null;
        
        return new AuthUserDTO
        {
            Id = authUser.Id,
            Username = authUser.Username,
            PasswordHash = authUser.PasswordHash,
            Role = (EnumAuthDTO)(int)authUser.Role
        };
    }

    public static AuthUser ToDomain(AuthUserDTO dto)
    {
        if (dto == null) return null;
        
        return new AuthUser
        {
            Id = dto.Id,
            Username = dto.Username,
            PasswordHash = dto.PasswordHash,
            Role = (EnumAuth)(int)dto.Role
        };
    }

    public static IEnumerable<AuthUserDTO> ToDTO(IEnumerable<AuthUser> authUsers)
    {
        return authUsers?.Select(ToDTO) ?? Enumerable.Empty<AuthUserDTO>();
    }
}

