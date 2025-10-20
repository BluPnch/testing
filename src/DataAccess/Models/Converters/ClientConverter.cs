using Domain.Models;


namespace DataAccess.Models.Converters;


public static class ClientConverter
{
    public static Client? ToDomain(this ClientDb? client)
    {
        if (client is null) return null;

        return new Client(
            id: client.Id,
            phoneNumber: client.PhoneNumber,
            companyName: client.CompanyName
        );
    }

    public static ClientDb? ToDb(this Client? client)
    {
        if (client is null) return null;

        return new ClientDb(
            id: client.Id,
            phoneNumber: client.PhoneNumber,
            companyName: client.CompanyName
        );
    }

    public static IEnumerable<Client> ToDomain(this IEnumerable<ClientDb> clients)
        => clients.Select(c => c.ToDomain())!;
}