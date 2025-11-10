using Domain.Models;
using DefaultNamespace;
using Server.Controllers.Models;
using EnumFlowersDTO = Server.Controllers.Models.Enums.EnumFlowers;
using EnumFruitDTO = Server.Controllers.Models.Enums.EnumFruit;
using EnumReproductionDTO = Server.Controllers.Models.Enums.EnumReproduction;

namespace Server.Controllers.Converters;

public static class PlantConverter
{
    public static PlantDTO ToDTO(Plant plant)
    {
        if (plant == null) return null;
        
        return new PlantDTO
        {
            Id = plant.Id,
            ClientId = plant.ClientId,
            Specie = plant.Specie,
            Family = plant.Family,
            Flower = (EnumFlowersDTO)(int)plant.Flower,
            Fruit = (EnumFruitDTO)(int)plant.Fruit,
            Reproduction = (EnumReproductionDTO)(int)plant.Reproduction
        };
    }

    public static Plant ToDomain(PlantDTO dto)
    {
        if (dto == null) return null;
        
        return new Plant(dto.Id, dto.ClientId, dto.Specie, dto.Family, (EnumFlowers)(int)dto.Flower, (EnumFruit)(int)dto.Fruit, (EnumReproduction)(int)dto.Reproduction);
    }

    public static IEnumerable<PlantDTO> ToDTO(IEnumerable<Plant> plants)
    {
        return plants?.Select(ToDTO) ?? Enumerable.Empty<PlantDTO>();
    }
}

