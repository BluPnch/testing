using DataAccess.Models;
using Domain.Models;
using UnitTests.Builders;


namespace UnitTests.MotherObjects
{
    public static class JournalRecordMotherObject
    {
        public static JournalRecord CreateDefaultJournalRecord()
        {
            return new JournalRecordBuilder().Build();
        }

        public static JournalRecordDb CreateDefaultJournalRecordDb()
        {
            return new JournalRecordDbBuilder().Build();
        }

        public static JournalRecordDb CreateJournalRecordDbWithPlantId(Guid plantId)
        {
            return new JournalRecordDbBuilder()
                .WithPlantId(plantId)
                .Build();
        }

        public static JournalRecordDb CreateJournalRecordDbWithCondition(EnumCondition condition)
        {
            return new JournalRecordDbBuilder()
                .WithCondition(condition)
                .Build();
        }

        public static JournalRecord CreateJournalRecordWithHeight(double height)
        {
            return new JournalRecordBuilder()
                .WithPlantHeight(height)
                .Build();
        }
    }
}