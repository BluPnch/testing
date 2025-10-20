using Domain.Models;

namespace UnitTests.Builders
{
    public class EmployeeBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _surname = "BBB";
        private string _name = "AAA";
        private string? _patronymic = "CCC";
        private string _task = "gardener";
        private string _plantDomain = "Roses";
        private string _phoneNumber = "0000000000";
        private Guid _administratorId = Guid.NewGuid();

        public EmployeeBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public EmployeeBuilder WithSurname(string surname)
        {
            _surname = surname;
            return this;
        }

        public EmployeeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public EmployeeBuilder WithPatronymic(string? patronymic)
        {
            _patronymic = patronymic;
            return this;
        }

        public EmployeeBuilder WithTask(string task)
        {
            _task = task;
            return this;
        }

        public EmployeeBuilder WithPlantDomain(string plantDomain)
        {
            _plantDomain = plantDomain;
            return this;
        }

        public EmployeeBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public EmployeeBuilder WithAdministratorId(Guid administratorId)
        {
            _administratorId = administratorId;
            return this;
        }

        public Employee Build()
        {
            return new Employee(_id, _surname, _name, _patronymic, _task, _plantDomain, _phoneNumber, _administratorId);
        }

        public static implicit operator Employee(EmployeeBuilder builder)
        {
            return builder.Build();
        }
    }
}