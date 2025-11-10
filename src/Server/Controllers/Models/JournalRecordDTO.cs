namespace Server.Controllers.Models;

public class JournalRecordDTO : BaseModelDTO
{
    public Guid PlantId { get; set; }
    public Guid GrowthStageId { get; set; }
    public Guid EmployeeId { get; set; }
    public double PlantHeight { get; set; }
    public int FruitCount { get; set; }
    public Enums.EnumCondition Condition { get; set; }
    public DateTimeOffset Date { get; set; }
}
