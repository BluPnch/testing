using Domain.Models;
using Server.Controllers.Models;

namespace Server.Controllers.Converters;

public static class AdministratorConverter
{
    public static AdministratorDTO ToDTO(Administrator administrator)
    {
        if (administrator == null) return null;
        
        return new AdministratorDTO
        {
            Id = administrator.Id,
            Surname = administrator.Surname,
            Name = administrator.Name,
            Patronymic = administrator.Patronymic,
            PhoneNumber = administrator.PhoneNumber,
            Username = administrator.Username
        };
    }

    public static Administrator ToDomain(AdministratorDTO dto)
    {
        if (dto == null) return null;
        
        return new Administrator(dto.Id, dto.Surname, dto.Name, dto.Patronymic, dto.PhoneNumber, dto.Username);
    }

    public static IEnumerable<AdministratorDTO> ToDTO(IEnumerable<Administrator> administrators)
    {
        return administrators?.Select(ToDTO) ?? Enumerable.Empty<AdministratorDTO>();
    }
}

