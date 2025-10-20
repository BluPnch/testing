using DefaultNamespace;
using Domain.Models;

namespace UnitTests.Builders
{
    public class PlantBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _clientId = Guid.NewGuid();
        private string _specie = "Rose";
        private string _family = "Rosaceae";
        private EnumFlowers _flower = EnumFlowers.Actinomorphic;
        private EnumFruit _fruit = EnumFruit.Berry;
        private EnumReproduction _reproduction = EnumReproduction.Bulbs;

        public PlantBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public PlantBuilder WithClientId(Guid clientId)
        {
            _clientId = clientId;
            return this;
        }

        public PlantBuilder WithSpecie(string specie)
        {
            _specie = specie;
            return this;
        }

        public PlantBuilder WithFamily(string family)
        {
            _family = family;
            return this;
        }

        public PlantBuilder WithFlower(EnumFlowers flower)
        {
            _flower = flower;
            return this;
        }

        public PlantBuilder WithFruit(EnumFruit fruit)
        {
            _fruit = fruit;
            return this;
        }

        public PlantBuilder WithReproduction(EnumReproduction reproduction)
        {
            _reproduction = reproduction;
            return this;
        }

        public Plant Build()
        {
            return new Plant(_id, _clientId, _specie, _family, _flower, _fruit, _reproduction);
        }

        public static implicit operator Plant(PlantBuilder builder)
        {
            return builder.Build();
        }
    }
}