using DataAccess.Models;
using Domain.Models;
using UnitTests.Builder;
using UnitTests.Builders;


namespace UnitTests.ObjectMother
{
    public static class GrowthStageMotherObject
    {
        public static GrowthStage CreateDefaultGrowthStage()
        {
            return new GrowthStageBuilder().Build();
        }

        public static GrowthStage CreateGrowthStageWithName(string name)
        {
            return new GrowthStageBuilder()
                .WithName(name)
                .Build();
        }

        public static GrowthStage CreateGrowthStageWithDescription(string description)
        {
            return new GrowthStageBuilder()
                .WithDescription(description)
                .Build();
        }

        public static GrowthStageDb CreateDefaultGrowthStageDb()
        {
            return new GrowthStageDbBuilder().Build();
        }

        public static GrowthStageDb CreateGrowthStageDbWithName(string name)
        {
            return new GrowthStageDbBuilder()
                .WithName(name)
                .Build();
        }

        public static GrowthStageDb CreateGrowthStageDbWithDescription(string description)
        {
            return new GrowthStageDbBuilder()
                .WithDescription(description)
                .Build();
        }

        public static GrowthStageDb CreateGerminationStage()
        {
            return new GrowthStageDbBuilder()
                .WithName("Germination")
                .WithDescription("Initial growth stage")
                .Build();
        }

        public static GrowthStageDb CreateVegetativeStage()
        {
            return new GrowthStageDbBuilder()
                .WithName("Vegetative")
                .WithDescription("Leaf growth stage")
                .Build();
        }

        public static GrowthStageDb CreateFloweringStage()
        {
            return new GrowthStageDbBuilder()
                .WithName("Flowering")
                .WithDescription("Bloom stage")
                .Build();
        }
    }
}