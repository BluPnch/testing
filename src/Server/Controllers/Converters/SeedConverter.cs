using Domain.Models;
using Domain.Models.Enums;
using Server.Controllers.Models;
using EnumViabilityDTO = Server.Controllers.Models.Enums.EnumViability;
using EnumLightDTO = Server.Controllers.Models.Enums.EnumLight;

namespace Server.Controllers.Converters;

public static class SeedConverter
{
    public static SeedDTO ToDTO(Seed seed)
    {
        if (seed == null) return null;
        
        return new SeedDTO
        {
            Id = seed.Id,
            PlantId = seed.PlantId,
            Maturity = seed.Maturity,
            Viability = (EnumViabilityDTO)(int)seed.Viability,
            LightRequirements = (EnumLightDTO)(int)seed.LightRequirements,
            WaterRequirements = seed.WaterRequirements,
            TemperatureRequirements = seed.TemperatureRequirements
        };
    }

    public static Seed ToDomain(SeedDTO dto)
    {
        if (dto == null) return null;
        
        return new Seed(dto.Id, dto.PlantId, dto.Maturity, (EnumViability)(int)dto.Viability, (EnumLight)(int)dto.LightRequirements, dto.WaterRequirements, dto.TemperatureRequirements);
    }

    public static IEnumerable<SeedDTO> ToDTO(IEnumerable<Seed> seeds)
    {
        return seeds?.Select(ToDTO) ?? Enumerable.Empty<SeedDTO>();
    }
}

