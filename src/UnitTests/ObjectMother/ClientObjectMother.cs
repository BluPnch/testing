using Domain.Models;
using DataAccess.Models;
using DefaultNamespace;


namespace UnitTests.ObjectMother
{
    public static class ClientObjectMother
    {
        public static Client CreateDefaultClient()
        {
            return new Client(
                Guid.NewGuid(),
                "Company",
                "1234567890"
            );
        }

        public static Client CreateClientWithCompanyName(string companyName)
        {
            return new Client(
                Guid.NewGuid(),
                companyName,
                "0000000000"
            );
        }

        public static Client CreateClientWithPhoneNumber(string phoneNumber)
        {
            return new Client(
                Guid.NewGuid(),
                "Company_WithPhoneNumber",
                phoneNumber
            );
        }

        public static ClientDb CreateDefaultClientDb()
        {
            return new ClientDb(
                Guid.NewGuid(),
                "Company_DefaultDb",
                "0000000000"
            );
        }

        public static ClientDb CreateClientDbWithCompanyName(string companyName)
        {
            return new ClientDb(
                Guid.NewGuid(),
                companyName,
                "0000000000"
            );
        }

        public static ClientDb CreateClientDbWithPhoneNumber(string phoneNumber)
        {
            return new ClientDb(
                Guid.NewGuid(),
                "Company",
                phoneNumber
            );
        }

        public static List<ClientDb> CreateClientsList(int count, string phoneNumber = null)
        {
            var clients = new List<ClientDb>();
            for (int i = 0; i < count; i++)
            {
                clients.Add(new ClientDb(
                    Guid.NewGuid(),
                    $"Company {i + 1}",
                    phoneNumber ?? $"{1000000000 + i}"
                ));
            }
            return clients;
        }
        
        public static Client CreateClientWithSpecificId(Guid id)
        {
            return new Client(
                id,
                "Company_SpecificId",
                "9999999999"
            );
        }

        public static List<Client> CreateClientsDomainList(int count, string phoneNumber = null)
        {
            var clients = new List<Client>();
            for (int i = 0; i < count; i++)
            {
                clients.Add(new Client(
                    Guid.NewGuid(),
                    $"Company {i + 1}",
                    phoneNumber ?? $"{1000000000 + i}"
                ));
            }
            return clients;
        }

        public static List<Plant> CreatePlantsList(Guid clientId, int count)
        {
            var plants = new List<Plant>();
            for (int i = 0; i < count; i++)
            {
                plants.Add(new Plant(
                    Guid.NewGuid(),
                    clientId,
                    $"Type{i + 1}",
                    $"Specie{i + 1}",
                    EnumFlowers.Actinomorphic,
                    EnumFruit.Berry,
                    EnumReproduction.Layering
                ));
            }
            return plants;
        }
    }
}