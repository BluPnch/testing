using Domain.Models;
using Domain.Models.Enums;

namespace UnitTests.Builders
{
    public class SeedBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _plantId = Guid.NewGuid();
        private string _maturity = "Mature";
        private EnumViability _viability = EnumViability.Contaminated;
        private EnumLight _lightRequirements = EnumLight.Medium;
        private string _waterRequirements = "Normal";
        private int _temperatureRequirements = 25;

        public SeedBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public SeedBuilder WithPlantId(Guid plantId)
        {
            _plantId = plantId;
            return this;
        }

        public SeedBuilder WithMaturity(string maturity)
        {
            _maturity = maturity;
            return this;
        }

        public SeedBuilder WithViability(EnumViability viability)
        {
            _viability = viability;
            return this;
        }

        public SeedBuilder WithLightRequirements(EnumLight lightRequirements)
        {
            _lightRequirements = lightRequirements;
            return this;
        }

        public SeedBuilder WithWaterRequirements(string waterRequirements)
        {
            _waterRequirements = waterRequirements;
            return this;
        }

        public SeedBuilder WithTemperatureRequirements(int temperatureRequirements)
        {
            _temperatureRequirements = temperatureRequirements;
            return this;
        }

        public Seed Build()
        {
            return new Seed(_id, _plantId, _maturity, _viability, 
                _lightRequirements, _waterRequirements, _temperatureRequirements);
        }

        public static implicit operator Seed(SeedBuilder builder)
        {
            return builder.Build();
        }
    }
}