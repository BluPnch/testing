using Domain.Models;
using Server.Controllers.Models;

namespace Server.Controllers.Converters;

public static class ClientConverter
{
    public static ClientDTO ToDTO(Client client)
    {
        if (client == null) return null;
        
        return new ClientDTO
        {
            Id = client.Id,
            CompanyName = client.CompanyName,
            PhoneNumber = client.PhoneNumber
        };
    }

    public static Client ToDomain(ClientDTO dto)
    {
        if (dto == null) return null;
        
        return new Client(dto.Id, dto.CompanyName, dto.PhoneNumber);
    }

    public static IEnumerable<ClientDTO> ToDTO(IEnumerable<Client> clients)
    {
        return clients?.Select(ToDTO) ?? Enumerable.Empty<ClientDTO>();
    }
}

