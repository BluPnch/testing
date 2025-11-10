using Domain.Models;


namespace DataAccess.Models.Converters;


public static class PlantConverter
{
    public static Plant? ToDomain(this PlantDb? plant)
    {
        if (plant is null) return null;

        return new Plant(
            id: plant.Id,
            clientId: plant.ClientId,
            specie: plant.Specie,
            family: plant.Family,
            flower: plant.Flower,
            fruit: plant.Fruit,
            reproduction: plant.Reproduction
        );
    }

    public static PlantDb? ToDb(this Plant? plant)
    {
        if (plant is null) return null;

        return new PlantDb(
            id: plant.Id,
            clientId: plant.ClientId,
            specie: plant.Specie,
            family: plant.Family,
            flower: plant.Flower,
            fruit: plant.Fruit,
            reproduction: plant.Reproduction
        );
    }

    public static IEnumerable<Plant> ToDomain(this IEnumerable<PlantDb> plants)
        => plants.Select(p => p.ToDomain())!;
}