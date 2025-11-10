using Domain.Models;
using UnitTests.Builders;


namespace UnitTests.ObjectMother
{
    public static class PlantMotherObject
    {
        public static Plant CreateDefaultPlant()
        {
            return new PlantBuilder().Build();
        }

        public static Plant CreatePlantWithId(Guid id)
        {
            return new PlantBuilder()
                .WithId(id)
                .Build();
        }

        public static Plant CreatePlantWithSpecie(string specie)
        {
            return new PlantBuilder()
                .WithSpecie(specie)
                .Build();
        }
    }
}