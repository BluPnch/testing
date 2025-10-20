using DataAccess.Models;
using DefaultNamespace;


namespace UnitTests.Builders
{
    public class PlantDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _clientId = Guid.NewGuid();
        private string _plantSpecie = "Specie";
        private string _plantFamily = "Family";
        private EnumFlowers _flowerType = EnumFlowers.Actinomorphic;
        private EnumFruit _fruitType = EnumFruit.Berry;
        private EnumReproduction _reproductionType = EnumReproduction.Layering;

        public PlantDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public PlantDbBuilder WithClientId(Guid clientId)
        {
            _clientId = clientId;
            return this;
        }

        public PlantDbBuilder WithPlantSpecie(string plantSpecie)
        {
            _plantSpecie = plantSpecie;
            return this;
        }
        
        public PlantDbBuilder WithPlantFamily(string plantFamily)
        {
            _plantFamily = plantFamily;
            return this;
        }

        public PlantDbBuilder WithFlowerType(EnumFlowers flowerType)
        {
            _flowerType = flowerType;
            return this;
        }

        public PlantDbBuilder WithFruitType(EnumFruit fruitType)
        {
            _fruitType = fruitType;
            return this;
        }

        public PlantDbBuilder WithReproductionType(EnumReproduction reproductionType)
        {
            _reproductionType = reproductionType;
            return this;
        }

        public PlantDb Build()
        {
            return new PlantDb(_id, _clientId, _plantSpecie, _plantFamily, _flowerType, _fruitType, _reproductionType);
        }

        public static implicit operator PlantDb(PlantDbBuilder builder)
        {
            return builder.Build();
        }
    }
}