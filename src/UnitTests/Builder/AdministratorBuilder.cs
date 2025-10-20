using Domain.Models;

namespace UnitTests.Builders
{
    public class AdministratorBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _surname = "BBB";
        private string _name = "AAA";
        private string? _patronymic = "CCC";
        private string _phoneNumber = "0000000000";
        private string _username = "username";

        public AdministratorBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public AdministratorBuilder WithSurname(string surname)
        {
            _surname = surname;
            return this;
        }

        public AdministratorBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AdministratorBuilder WithPatronymic(string? patronymic)
        {
            _patronymic = patronymic;
            return this;
        }

        public AdministratorBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public AdministratorBuilder WithUsername(string username)
        {
            _username = username;
            return this;
        }

        public Administrator Build()
        {
            return new Administrator(_id, _surname, _name, _patronymic, _phoneNumber, _username);
        }

        public static implicit operator Administrator(AdministratorBuilder builder)
        {
            return builder.Build();
        }
    }
}