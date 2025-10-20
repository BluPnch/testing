using Domain.Models;
using DataAccess.Models;
using Domain.Models.Enums;


namespace UnitTests.ObjectMother
{
    public static class AdministratorObjectMother
    {
        public static Administrator CreateDefaultAdministrator()
        {
            return new Administrator(
                Guid.NewGuid(),
                "BBB_Default",
                "AAA_Default",
                "CCC_Default",
                "0000000000",
                "user_Default"
            );
        }

        public static Administrator CreateAdministratorWithPhone(string phoneNumber)
        {
            return new Administrator(
                Guid.NewGuid(),
                "BBB_WithPhone",
                "AAA_WithPhone",
                "CCC_WithPhone",
                phoneNumber,
                "user_WithPhone"
            );
        }

        public static Administrator CreateAdministratorWithFullName(string surname, string name, string patronymic)
        {
            return new Administrator(
                Guid.NewGuid(),
                surname,
                name,
                patronymic,
                "0000000000",
                $"{name.ToLower()}{surname.ToLower()}"
            );
        }

        public static Administrator CreateAdministratorWithoutPatronymic()
        {
            return new Administrator(
                Guid.NewGuid(),
                "BBB_WithoutPatronymic",
                "AAA_WithoutPatronymi",
                null,
                "0000000000",
                "user_WithoutPatronymi"
            );
        }

        public static AdministratorDb CreateDefaultAdministratorDb()
        {
            return new AdministratorDb(
                Guid.NewGuid(),
                "BBB_Default_Db",
                "AAA_Default_Db",
                "CCC_Default_Db",
                "0000000000",
                "user_Default_Db"
            );
        }

        public static AdministratorDb CreateAdministratorDbWithPhone(string phoneNumber)
        {
            return new AdministratorDb(
                Guid.NewGuid(),
                "BBB_WithPhone_Db",
                "AAA_WithPhone_Db",
                "CCC_WithPhone_Db",
                phoneNumber,
                "user_WithPhone_Db"
            );
        }

        public static AdministratorDb CreateAdministratorDbWithFullName(string surname, string name, string patronymic)
        {
            return new AdministratorDb(
                Guid.NewGuid(),
                surname,
                name,
                patronymic,
                "0000000000",
                $"{name.ToLower()}{surname.ToLower()}"
            );
        }
        
        public static Administrator CreateAdministratorWithCredentials(string username, string password = "defaultPassword")
        {
            return new Administrator(
                Guid.NewGuid(),
                "BBB_WithCredentials",
                "AAA_WithCredentials",
                "CCC_WithCredentials",
                "0000000000",
                username
            );
        }

        public static Administrator CreateAdministratorWithSpecificId(Guid id)
        {
            return new Administrator(
                id,
                "BBB_SpecificId",
                "AAA_SpecificId",
                "CCC_SpecificId",
                "0000000000",
                "user_SpecificId"
            );
        }

        public static AuthUser CreateAuthUserForAdministrator(Guid adminId, string username, string passwordHash)
        {
            return new AuthUser
            {
                Id = adminId,
                Username = username,
                PasswordHash = passwordHash,
                Role = EnumAuth.Administrator
            };
        }

        public static List<Administrator> CreateAdministratorsList(int count)
        {
            var administrators = new List<Administrator>();
            for (int i = 0; i < count; i++)
            {
                administrators.Add(new Administrator(
                    Guid.NewGuid(),
                    $"BBB{i}",
                    $"AAA{i}",
                    i % 2 == 0 ? $"CCC{i}" : null,
                    $"{1000000000 + i}",
                    $"user{i}"
                ));
            }
            return administrators;
        }
    }
}