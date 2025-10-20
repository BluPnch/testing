using Domain.Models;

namespace UnitTests.Builders
{
    public class GrowthStageBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _name = "Germination";
        private string _description = "Initial growth stage";

        public GrowthStageBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public GrowthStageBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public GrowthStageBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public GrowthStage Build()
        {
            return new GrowthStage(_id, _name, _description);
        }

        public static implicit operator GrowthStage(GrowthStageBuilder builder)
        {
            return builder.Build();
        }
    }
}