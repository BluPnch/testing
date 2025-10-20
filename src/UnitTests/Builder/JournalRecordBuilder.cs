using Domain.Models;


namespace UnitTests.Builders
{
    public class JournalRecordBuilder
    {
        private Guid _id = Guid.NewGuid();
        private Guid _plantId = Guid.NewGuid();
        private Guid _growthStageId = Guid.NewGuid();
        private Guid _employeeId = Guid.NewGuid();
        private double _plantHeight = 10.5;
        private int _fruitCount = 5;
        private EnumCondition _condition = EnumCondition.Healthy;
        private DateTimeOffset _date = DateTimeOffset.Now;

        public JournalRecordBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public JournalRecordBuilder WithPlantId(Guid plantId)
        {
            _plantId = plantId;
            return this;
        }

        public JournalRecordBuilder WithGrowthStageId(Guid growthStageId)
        {
            _growthStageId = growthStageId;
            return this;
        }

        public JournalRecordBuilder WithEmployeeId(Guid employeeId)
        {
            _employeeId = employeeId;
            return this;
        }

        public JournalRecordBuilder WithPlantHeight(double plantHeight)
        {
            _plantHeight = plantHeight;
            return this;
        }

        public JournalRecordBuilder WithFruitCount(int fruitCount)
        {
            _fruitCount = fruitCount;
            return this;
        }

        public JournalRecordBuilder WithCondition(EnumCondition condition)
        {
            _condition = condition;
            return this;
        }

        public JournalRecordBuilder WithDate(DateTimeOffset date)
        {
            _date = date;
            return this;
        }

        public JournalRecord Build()
        {
            return new JournalRecord(_id, _plantId, _growthStageId, _employeeId, 
                _plantHeight, _fruitCount, _condition, _date);
        }

        public static implicit operator JournalRecord(JournalRecordBuilder builder)
        {
            return builder.Build();
        }
    }
}