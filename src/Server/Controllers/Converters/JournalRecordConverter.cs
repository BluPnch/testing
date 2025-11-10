using Domain.Models;
using Domain.Models.Enums;
using Server.Controllers.Models;
using EnumConditionDTO = Server.Controllers.Models.Enums.EnumCondition;

namespace Server.Controllers.Converters;

public static class JournalRecordConverter
{
    public static JournalRecordDTO ToDTO(JournalRecord journalRecord)
    {
        if (journalRecord == null) return null;
        
        return new JournalRecordDTO
        {
            Id = journalRecord.Id,
            PlantId = journalRecord.PlantId,
            GrowthStageId = journalRecord.GrowthStageId,
            EmployeeId = journalRecord.EmployeeId,
            PlantHeight = journalRecord.PlantHeight,
            FruitCount = journalRecord.FruitCount,
            Condition = (EnumConditionDTO)(int)journalRecord.Condition,
            Date = journalRecord.Date
        };
    }

    public static JournalRecord ToDomain(JournalRecordDTO dto)
    {
        if (dto == null) return null;
        
        return new JournalRecord(dto.Id, dto.PlantId, dto.GrowthStageId, dto.EmployeeId, dto.PlantHeight, dto.FruitCount, (EnumCondition)(int)dto.Condition, dto.Date);
    }

    public static IEnumerable<JournalRecordDTO> ToDTO(IEnumerable<JournalRecord> journalRecords)
    {
        return journalRecords?.Select(ToDTO) ?? Enumerable.Empty<JournalRecordDTO>();
    }
}

