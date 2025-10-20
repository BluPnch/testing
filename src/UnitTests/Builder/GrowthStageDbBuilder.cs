using DataAccess.Models;

namespace UnitTests.Builder
{
    public class GrowthStageDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Germination";
        private string _description = "Initial growth stage";

        public GrowthStageDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public GrowthStageDbBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public GrowthStageDbBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public GrowthStageDb Build()
        {
            return new GrowthStageDb(_id, _name, _description);
        }

        public static implicit operator GrowthStageDb(GrowthStageDbBuilder builder)
        {
            return builder.Build();
        }
    }
}