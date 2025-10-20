using Domain.Models;


namespace DataAccess.Models.Converters;

public static class SeedConverter
{
    public static Seed? ToDomain(this SeedDb? seed)
    {
        if (seed is null) return null;
        return new Seed(
            id: seed.Id,
            plantId: seed.PlantId,
            maturity: seed.Maturity,
            viability: seed.Viability,
            lightRequirements: seed.LightRequirements,
            waterRequirements: seed.WaterRequirements,
            temperatureRequirements: seed.TemperatureRequirements
        );
    }

    public static SeedDb? ToDb(this Seed? seed)
    {
        if (seed is null) return null;
        return new SeedDb(
            id: seed.Id,
            plantId: seed.PlantId,
            maturity: seed.Maturity,
            viability: seed.Viability,
            lightRequirements: seed.LightRequirements,
            waterRequirements: seed.WaterRequirements,
            temperatureRequirements: seed.TemperatureRequirements
        );
    }

    public static IEnumerable<Seed> ToDomain(this IEnumerable<SeedDb> seeds)
        => seeds.Select(s => s.ToDomain())!;
}
