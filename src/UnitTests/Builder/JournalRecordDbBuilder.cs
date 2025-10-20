using DataAccess.Models;


namespace UnitTests.Builders
{
    public class JournalRecordDbBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _plantId = Guid.NewGuid();
        private Guid _growthStageId = Guid.NewGuid();
        private Guid _employeeId = Guid.NewGuid();
        private double _plantHeight = 10.5;
        private int _fruitCount = 5;
        private EnumCondition _condition = EnumCondition.Healthy;
        private DateTimeOffset _date = DateTimeOffset.Now;

        public JournalRecordDbBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public JournalRecordDbBuilder WithPlantId(Guid plantId)
        {
            _plantId = plantId;
            return this;
        }

        public JournalRecordDbBuilder WithGrowthStageId(Guid growthStageId)
        {
            _growthStageId = growthStageId;
            return this;
        }

        public JournalRecordDbBuilder WithEmployeeId(Guid employeeId)
        {
            _employeeId = employeeId;
            return this;
        }

        public JournalRecordDbBuilder WithPlantHeight(double plantHeight)
        {
            _plantHeight = plantHeight;
            return this;
        }

        public JournalRecordDbBuilder WithFruitCount(int fruitCount)
        {
            _fruitCount = fruitCount;
            return this;
        }

        public JournalRecordDbBuilder WithCondition(EnumCondition condition)
        {
            _condition = condition;
            return this;
        }

        public JournalRecordDbBuilder WithDate(DateTimeOffset date)
        {
            _date = date;
            return this;
        }

        public JournalRecordDb Build()
        {
            return new JournalRecordDb(_id, _plantHeight, _fruitCount, _condition, 
                _date, _plantId, _growthStageId, _employeeId);
        }

        public static implicit operator JournalRecordDb(JournalRecordDbBuilder builder)
        {
            return builder.Build();
        }
    }
}