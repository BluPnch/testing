using DataAccess.Models;

namespace UnitTests.Builders
{
    public class EmployeeDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _surname = "BBB";
        private string _name = "AAA";
        private string? _patronymic = "CCC";
        private string _task = "gardener";
        private string _plantDomain = "Roses";
        private string _phoneNumber = "0000000000";
        private Guid _administratorId = Guid.NewGuid();
        private AdministratorDb? _administrator;

        public EmployeeDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public EmployeeDbBuilder WithSurname(string surname)
        {
            _surname = surname;
            return this;
        }

        public EmployeeDbBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public EmployeeDbBuilder WithPatronymic(string? patronymic)
        {
            _patronymic = patronymic;
            return this;
        }

        public EmployeeDbBuilder WithTask(string task)
        {
            _task = task;
            return this;
        }

        public EmployeeDbBuilder WithPlantDomain(string plantDomain)
        {
            _plantDomain = plantDomain;
            return this;
        }

        public EmployeeDbBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public EmployeeDbBuilder WithAdministratorId(Guid administratorId)
        {
            _administratorId = administratorId;
            return this;
        }

        public EmployeeDbBuilder WithAdministrator(AdministratorDb administrator)
        {
            _administrator = administrator;
            _administratorId = administrator.Id;
            return this;
        }

        public EmployeeDb Build()
        {
            var employee = new EmployeeDb(_id, _surname, _name, _patronymic, _task, _plantDomain, _phoneNumber)
            {
                AdministratorId = _administratorId,
                Administrator = _administrator
            };

            return employee;
        }

        public static implicit operator EmployeeDb(EmployeeDbBuilder builder)
        {
            return builder.Build();
        }
    }
}