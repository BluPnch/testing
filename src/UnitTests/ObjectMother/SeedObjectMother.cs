using DataAccess.Models;
using Domain.Models;
using UnitTests.Builders;


namespace UnitTests.MotherObjects
{
    public static class SeedMotherObject
    {
        public static Seed CreateDefaultSeed()
        {
            return new SeedBuilder().Build();
        }

        public static SeedDb CreateDefaultSeedDb()
        {
            return new SeedDbBuilder().Build();
        }

        public static SeedDb CreateSeedDbWithMaturity(string maturity)
        {
            return new SeedDbBuilder()
                .WithMaturity(maturity)
                .Build();
        }

        public static SeedDb CreateSeedDbWithViability(EnumViability viability)
        {
            return new SeedDbBuilder()
                .WithViability(viability)
                .Build();
        }

        public static SeedDb CreateSeedDbWithPlantId(Guid plantId)
        {
            return new SeedDbBuilder()
                .WithPlantId(plantId)
                .Build();
        }

        public static SeedDb CreateMatureSeed()
        {
            return new SeedDbBuilder()
                .WithMaturity("Mature")
                .WithViability(EnumViability.Contaminated)
                .Build();
        }

        public static SeedDb CreateImmatureSeed()
        {
            return new SeedDbBuilder()
                .WithMaturity("Immature")
                .WithViability(EnumViability.Damaged)
                .Build();
        }
    }
}