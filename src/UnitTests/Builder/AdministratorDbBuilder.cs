using DataAccess.Models;

namespace UnitTests.Builders
{
    public class AdministratorDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _surname = "BBB";
        private string _name = "AAA";
        private string? _patronymic = "CCC";
        private string _phoneNumber = "0000000000";
        private string _username = "username";

        public AdministratorDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public AdministratorDbBuilder WithSurname(string surname)
        {
            _surname = surname;
            return this;
        }

        public AdministratorDbBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AdministratorDbBuilder WithPatronymic(string? patronymic)
        {
            _patronymic = patronymic;
            return this;
        }

        public AdministratorDbBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public AdministratorDbBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public AdministratorDb Build()
        {
            return new AdministratorDb(_id, _surname, _name, _patronymic, _phoneNumber, _username);
        }

        public static implicit operator AdministratorDb(AdministratorDbBuilder builder)
        {
            return builder.Build();
        }
    }
}