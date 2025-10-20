using Domain.Models;

namespace DataAccess.Models.Converters;

public static class JournalRecordConverter
{
    public static JournalRecord? ToDomain(this JournalRecordDb? record)
    {
        if (record is null) return null;

        return new JournalRecord(
            id: record.Id,
            plantId: record.PlantId,
            growthStageId: record.GrowthStageId,
            employeeId: record.EmployeeId,
            plantHeight: record.PlantHeight,
            fruitCount: record.FruitCount,
            condition: record.Condition,
            date: record.Date
        );
    }

    public static JournalRecordDb? ToDb(this JournalRecord? record)
    {
        if (record is null) return null;

        return new JournalRecordDb(
            id: record.Id,
            plantHeight: record.PlantHeight,
            fruitCount: record.FruitCount,
            condition: record.Condition,
            date: record.Date,
            plantId: record.PlantId,
            growthStageId: record.GrowthStageId,
            employeeId: record.EmployeeId
        );
    }

    public static IEnumerable<JournalRecord> ToDomain(this IEnumerable<JournalRecordDb> records)
        => records.Select(r => r.ToDomain())!;
}