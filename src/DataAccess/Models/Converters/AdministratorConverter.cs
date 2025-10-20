using Domain.Models;


namespace DataAccess.Models.Converters;


public static class  AdministratorConverter
{
    public static Administrator? ToDomain(this AdministratorDb? admin)
    {
        if (admin is null) return null;

        return new Administrator(
            id: admin.Id,
            phoneNumber: admin.PhoneNumber,
            surname: admin.Surname,
            name: admin.Name,
            patronymic: admin.Patronymic,
            username: admin.Username
        );
    }

    public static AdministratorDb? ToDb(this Administrator? admin)
    {
        if (admin is null) return null;

        return new AdministratorDb(
            id: admin.Id,
            phoneNumber: admin.PhoneNumber,
            surname: admin.Surname,
            name: admin.Name,
            patronymic: admin.Patronymic,
            username: admin.Username
        );
    }

    public static IEnumerable<Administrator> ToDomain(this IEnumerable<AdministratorDb> admins)
        => admins.Select(a => a.ToDomain())!;
}