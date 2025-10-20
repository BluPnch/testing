using DataAccess.Models;
using Domain.Models.Enums;

namespace UnitTests.Builders
{
    public class SeedDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _plantId = Guid.NewGuid();
        private string _maturity = "Mature";
        private EnumViability _viability = EnumViability.Contaminated;
        private EnumLight _lightRequirements = EnumLight.Medium;
        private string _waterRequirements = "Normal";
        private int _temperatureRequirements = 25;

        public SeedDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public SeedDbBuilder WithPlantId(Guid plantId)
        {
            _plantId = plantId;
            return this;
        }

        public SeedDbBuilder WithMaturity(string maturity)
        {
            _maturity = maturity;
            return this;
        }

        public SeedDbBuilder WithViability(EnumViability viability)
        {
            _viability = viability;
            return this;
        }

        public SeedDbBuilder WithLightRequirements(EnumLight lightRequirements)
        {
            _lightRequirements = lightRequirements;
            return this;
        }

        public SeedDbBuilder WithWaterRequirements(string waterRequirements)
        {
            _waterRequirements = waterRequirements;
            return this;
        }

        public SeedDbBuilder WithTemperatureRequirements(int temperatureRequirements)
        {
            _temperatureRequirements = temperatureRequirements;
            return this;
        }

        public SeedDb Build()
        {
            return new SeedDb(_id, _plantId, _maturity, _viability, 
                _lightRequirements, _waterRequirements, _temperatureRequirements);
        }

        public static implicit operator SeedDb(SeedDbBuilder builder)
        {
            return builder.Build();
        }
    }
}