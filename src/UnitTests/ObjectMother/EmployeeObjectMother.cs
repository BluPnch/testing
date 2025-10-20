using DataAccess.Models;
using Domain.Models;
using UnitTests.Builders;


namespace UnitTests.MotherObjects
{
    public static class EmployeeMotherObject
    {
        public static Employee CreateDefaultEmployee()
        {
            return new EmployeeBuilder().Build();
        }

        public static Employee CreateEmployeeWithTask(string task)
        {
            return new EmployeeBuilder()
                .WithTask(task)
                .Build();
        }

        public static Employee CreateEmployeeWithPlantDomain(string plantDomain)
        {
            return new EmployeeBuilder()
                .WithPlantDomain(plantDomain)
                .Build();
        }

        public static Employee CreateEmployeeWithPhoneNumber(string phoneNumber)
        {
            return new EmployeeBuilder()
                .WithPhoneNumber(phoneNumber)
                .Build();
        }

        public static EmployeeDb CreateDefaultEmployeeDb()
        {
            return new EmployeeDbBuilder().Build();
        }

        public static EmployeeDb CreateEmployeeDbWithAdministrator(AdministratorDb administrator)
        {
            return new EmployeeDbBuilder()
                .WithAdministrator(administrator)
                .Build();
        }

        public static EmployeeDb CreateEmployeeDbWithTask(string task)
        {
            return new EmployeeDbBuilder()
                .WithTask(task)
                .Build();
        }

        public static EmployeeDb CreateEmployeeDbWithPlantDomain(string plantDomain)
        {
            return new EmployeeDbBuilder()
                .WithPlantDomain(plantDomain)
                .Build();
        }

        public static EmployeeDb CreateEmployeeDbWithPhoneNumber(string phoneNumber)
        {
            return new EmployeeDbBuilder()
                .WithPhoneNumber(phoneNumber)
                .Build();
        }

        public static AdministratorDb CreateDefaultAdministratorDb()
        {
            return new AdministratorDbBuilder().Build();
        }

        public static (EmployeeDb employee, AdministratorDb administrator) CreateEmployeeWithAdministrator()
        {
            var administrator = CreateDefaultAdministratorDb();
            var employee = CreateEmployeeDbWithAdministrator(administrator);
            
            return (employee, administrator);
        }
    }
}